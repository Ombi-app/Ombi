using System.Security.Principal;
using System.Threading.Tasks;
using Ombi.Core.Rule.Interfaces;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Rule.Rules.Specific
{
    public class SendNotificationRule : SpecificRule, ISpecificRule<object>
    {
        public SendNotificationRule(IPrincipal principal)
        {
            User = principal;
        }

        public override SpecificRules Rule => SpecificRules.CanSendNotification;
        private IPrincipal User { get; }

        public Task<RuleResult> Execute(object obj)
        {
            var req = (BaseRequest)obj;
            var sendNotification = !req.Approved; /*|| !prSettings.IgnoreNotifyForAutoApprovedRequests;*/

            if (req.RequestType == RequestType.Movie)
            {
                sendNotification = !User.IsInRole(OmbiRoles.AutoApproveMovie);
            }
            else if(req.RequestType ==  RequestType.TvShow)
            {
                sendNotification = !User.IsInRole(OmbiRoles.AutoApproveTv);
            }


            if (User.IsInRole(OmbiRoles.Admin))
            {
                sendNotification = false; // Don't bother sending a notification if the user is an admin
            }

 

            return Task.FromResult(new RuleResult
            {
                Success = sendNotification
            });
        }
    }
}