using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Core.Helpers;
using Ombi.Core.Models.Requests;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Services;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Rule.Rules.Request
{
    public class AutoApproveRule : BaseRequestRule, IRules<BaseRequest>
    {
        public AutoApproveRule(ICurrentUser principal, OmbiUserManager um, IFeatureService featureService)
        {
            User = principal;
            _manager = um;
            _featureService = featureService;
        }

        private ICurrentUser User { get; }
        private readonly OmbiUserManager _manager;
        private readonly IFeatureService _featureService;

        public async Task<RuleResult> Execute(BaseRequest obj)
        {
            var currentUser = await User.GetUser();
            var username = currentUser.UserName.ToUpper();
            var user = await _manager.Users.FirstOrDefaultAsync(x => x.NormalizedUserName == username);
            if (await _manager.IsInRoleAsync(user, OmbiRoles.Admin) || user.IsSystemUser)
            {
                if (obj is MovieRequests movie)
                {
                    await Check4K(movie);
                }
                else
                {
                    obj.Approved = true;
                }
                return Success();
            }

            if (obj.RequestType == RequestType.Movie && await _manager.IsInRoleAsync(user, OmbiRoles.AutoApproveMovie))
            {
                var movie = (MovieRequests)obj;
                await Check4K(movie);
            }
            if (obj.RequestType == RequestType.TvShow && await _manager.IsInRoleAsync(user, OmbiRoles.AutoApproveTv))
                obj.Approved = true;
            if (obj.RequestType == RequestType.Album && await _manager.IsInRoleAsync(user, OmbiRoles.AutoApproveMusic))
                obj.Approved = true;
            return Success(); // We don't really care, we just don't set the obj to approve
        }

        private async Task Check4K(MovieRequests movie)
        {
            var featureEnabled = await _featureService.FeatureEnabled(FeatureNames.Movie4KRequests);
            if (movie.Is4kRequest && featureEnabled)
            {
                movie.Approved4K = true;
            }
            else
            {
                movie.Approved = true;
            }
        }
    }
}