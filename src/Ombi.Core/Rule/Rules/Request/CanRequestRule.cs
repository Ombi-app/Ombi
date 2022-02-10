using System;
using Ombi.Store.Entities;
using System.IO;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Core.Engine;
using Ombi.Core.Rule.Interfaces;
using Ombi.Helpers;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Rule.Rules.Request
{
    public class CanRequestRule : BaseRequestRule, IRules<BaseRequest>
    {
        public CanRequestRule(IPrincipal principal, OmbiUserManager manager)
        {
            User = principal;
            _manager = manager;
        }

        private IPrincipal User { get; }
        private readonly OmbiUserManager _manager;

        public async Task<RuleResult> Execute(BaseRequest obj)
        {
            var username = User.Identity.Name.ToUpper();
            var user = await _manager.Users.FirstOrDefaultAsync(x => x.NormalizedUserName == username);
            if (await _manager.IsInRoleAsync(user, OmbiRoles.Admin) || user.IsSystemUser)
                return Success();

            if (obj.RequestType == RequestType.Movie)
            {
                var movie = (MovieRequests)obj;
                var hasAutoApprove = await _manager.IsInRoleAsync(user, OmbiRoles.AutoApproveMovie);
                if (await _manager.IsInRoleAsync(user, OmbiRoles.RequestMovie) || hasAutoApprove)
                {
                    if (movie.Has4KRequest && !hasAutoApprove)
                    {
                        var has4kPermission = await _manager.IsInRoleAsync(user, OmbiRoles.Request4KMovie);
                        if (has4kPermission)
                        {
                            return Success();
                        }
                    }
                    else
                    {
                        return Success();
                    }
                }
                return Fail(ErrorCode.NoPermissionsRequestMovie, "You do not have permissions to Request a Movie");
            }

            if (obj.RequestType == RequestType.TvShow)
            {
                if (await _manager.IsInRoleAsync(user, OmbiRoles.RequestTv) || await _manager.IsInRoleAsync(user, OmbiRoles.AutoApproveTv))
                {
                    return Success();
                }

                return Fail(ErrorCode.NoPermissionsRequestTV, "You do not have permissions to Request a TV Show");
            }

            if (obj.RequestType == RequestType.Album)
            {
                if (await _manager.IsInRoleAsync(user, OmbiRoles.RequestMusic) || await _manager.IsInRoleAsync(user, OmbiRoles.AutoApproveMusic))
                {
                    return Success();
                }

                return Fail(ErrorCode.NoPermissionsRequestAlbum, "You do not have permissions to Request an Album");
            }

            throw new InvalidDataException("Permission check failed: unknown RequestType");
        }
    }
}
