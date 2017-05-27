using Ombi.Core.Claims;
using Ombi.Core.Models.Requests;
using Ombi.Core.Rules;
using System.Security.Principal;

namespace Ombi.Core.Rule.Rules
{
    public class CanRequestRule<T> : BaseRule where T : BaseRequestModel, IRequestRules<T>
    {
        public CanRequestRule(IPrincipal principal)
        {
            User = principal;
        }

        private IPrincipal User { get; }
        public RuleResult Execute(T obj)
        {
            if (obj.Type == Store.Entities.RequestType.Movie)
            {
                if (User.IsInRole(OmbiClaims.RequestMovie))
                {
                    return Success();
                }
                return Fail("You do not have permissions to Request a Movie");
            }
            else
            {
                if (User.IsInRole(OmbiClaims.RequestTv))
                {
                    return Success();
                }
                return Fail("You do not have permissions to Request a Movie");
            }
        }
    }
}
