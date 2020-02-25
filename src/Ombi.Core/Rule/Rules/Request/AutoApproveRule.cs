using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Core.Models.Requests;
using Ombi.Core.Rule.Interfaces;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Rule.Rules.Request
{
    public class AutoApproveRule : BaseRequestRule, IRules<BaseRequest>
    {
        public AutoApproveRule(IPrincipal principal, OmbiUserManager um)
        {
            User = principal;
            _manager = um;
        }

        private IPrincipal User { get; }
        private readonly OmbiUserManager _manager;

        public async Task<RuleResult> Execute(BaseRequest obj)
        {
            var username = User.Identity.Name.ToUpper();
            var user = await _manager.Users.FirstOrDefaultAsync(x => x.NormalizedUserName == username);
            if (await _manager.IsInRoleAsync(user, OmbiRoles.Admin) || user.IsSystemUser)
            {
                obj.Approved = true;
                return Success();
            }

            if (obj.RequestType == RequestType.Movie && await _manager.IsInRoleAsync(user, OmbiRoles.AutoApproveMovie))
                obj.Approved = true;
            if (obj.RequestType == RequestType.TvShow && await _manager.IsInRoleAsync(user, OmbiRoles.AutoApproveTv))
                obj.Approved = true;
            if (obj.RequestType == RequestType.Album && await _manager.IsInRoleAsync(user, OmbiRoles.AutoApproveMusic))
                obj.Approved = true;
            return Success(); // We don't really care, we just don't set the obj to approve
        }
    }
}