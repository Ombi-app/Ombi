using System.Security.Principal;
using System.Threading.Tasks;
using Ombi.Core.Models.Requests;
using Ombi.Core.Rule.Interfaces;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Rule.Rules.Request
{
    public class AutoApproveRule : BaseRequestRule, IRules<BaseRequest>
    {
        public AutoApproveRule(IPrincipal principal)
        {
            User = principal;
        }

        private IPrincipal User { get; }

        public Task<RuleResult> Execute(BaseRequest obj)
        {
            if (User.IsInRole(OmbiRoles.Admin))
            {
                obj.Approved = true;
                return Task.FromResult(Success());
            }

            if (obj.RequestType == RequestType.Movie && User.IsInRole(OmbiRoles.AutoApproveMovie))
                obj.Approved = true;
            if (obj.RequestType == RequestType.TvShow && User.IsInRole(OmbiRoles.AutoApproveTv))
                obj.Approved = true;
            if (obj.RequestType == RequestType.Album && User.IsInRole(OmbiRoles.AutoApproveMusic))
                obj.Approved = true;
            return Task.FromResult(Success()); // We don't really care, we just don't set the obj to approve
        }
    }
}