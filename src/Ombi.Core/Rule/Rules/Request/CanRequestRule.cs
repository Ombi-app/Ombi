using Ombi.Core.Claims;
using Ombi.Core.Models.Requests;
using Ombi.Core.Rules;
using Ombi.Store.Entities;
using System.Security.Principal;
using Ombi.Core.Rule.Interfaces;

namespace Ombi.Core.Rule.Rules
{
    public class CanRequestRule : BaseRequestRule, IRequestRules<BaseRequestModel>
    {
        public CanRequestRule(IPrincipal principal)
        {
            User = principal;
        }

        private IPrincipal User { get; }

        public RuleResult Execute(BaseRequestModel obj)
        {
            if (User.IsInRole(OmbiClaims.Admin))
                return Success();

            if (obj.Type == RequestType.Movie)
            {
                if (User.IsInRole(OmbiClaims.RequestMovie))
                    return Success();
                return Fail("You do not have permissions to Request a Movie");
            }

            if (User.IsInRole(OmbiClaims.RequestTv))
                return Success();
            return Fail("You do not have permissions to Request a Movie");
        }
    }
}