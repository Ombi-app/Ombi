using Ombi.Core.Claims;
using Ombi.Core.Models.Requests;
using Ombi.Core.Rules;
using Ombi.Store.Entities;
using System.Security.Principal;

namespace Ombi.Core.Rule.Rules
{
    public class AutoApproveRule : BaseRule, IRequestRules<BaseRequestModel>
    {
        public AutoApproveRule(IPrincipal principal)
        {
            User = principal;
        }

        private IPrincipal User { get; }

        public RuleResult Execute(BaseRequestModel obj)
        {
            if (User.IsInRole(OmbiClaims.Admin))
            {
                obj.Approved = true;
                return Success();
            }

            if (obj.Type == RequestType.Movie && User.IsInRole(OmbiClaims.AutoApproveMovie))
                obj.Approved = true;
            if (obj.Type == RequestType.TvShow && User.IsInRole(OmbiClaims.AutoApproveTv))
                obj.Approved = true;
            return Success(); // We don't really care, we just don't set the obj to approve
        }
    }
}