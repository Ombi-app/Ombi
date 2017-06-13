using System.Security.Principal;
using Ombi.Core.Claims;
using Ombi.Core.Models.Requests;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities;

namespace Ombi.Core.Rule.Rules.Request
{
    public class AutoApproveRule : BaseRequestRule, IRequestRules<BaseRequestModel>
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