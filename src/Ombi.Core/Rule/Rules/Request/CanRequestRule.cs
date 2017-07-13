using Ombi.Core.Claims;
using Ombi.Store.Entities;
using System.Security.Principal;
using System.Threading.Tasks;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Rule.Rules
{
    public class CanRequestRule : BaseRequestRule, IRules<BaseRequest>
    {
        public CanRequestRule(IPrincipal principal)
        {
            User = principal;
        }

        private IPrincipal User { get; }

        public Task<RuleResult> Execute(BaseRequest obj)
        {
            if (User.IsInRole(OmbiRoles.Admin))
                return Task.FromResult(Success());

            if (obj.RequestType == RequestType.Movie)
            {
                if (User.IsInRole(OmbiRoles.RequestMovie))
                    return Task.FromResult(Success());
                return Task.FromResult(Fail("You do not have permissions to Request a Movie"));
            }

            if (User.IsInRole(OmbiRoles.RequestTv))
                return Task.FromResult(Success());
            return Task.FromResult(Fail("You do not have permissions to Request a Movie"));
        }
    }
}