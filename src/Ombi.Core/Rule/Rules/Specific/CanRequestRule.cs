using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Core.Rule.Interfaces;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Rule.Rules.Specific
{
    public class SendNotificationRule : SpecificRule, ISpecificRule<object>
    {
        public SendNotificationRule(OmbiUserManager um)
        {
            UserManager = um;
        }

        public override SpecificRules Rule => SpecificRules.CanSendNotification;
        private OmbiUserManager UserManager { get; }

        public async Task<RuleResult> Execute(object obj)
        {
            var req = (BaseRequest)obj;
            var sendNotification = !req.Approved; /*|| !prSettings.IgnoreNotifyForAutoApprovedRequests;*/
            var requestedUser = await UserManager.Users.FirstOrDefaultAsync(x => x.Id == req.RequestedUserId);
            if (req.RequestType == RequestType.Movie)
            {
                sendNotification = !await UserManager.IsInRoleAsync(requestedUser, OmbiRoles.AutoApproveMovie);
            }
            else if(req.RequestType ==  RequestType.TvShow)
            {
                sendNotification = !await UserManager.IsInRoleAsync(requestedUser, OmbiRoles.AutoApproveTv);
            }

            if (await UserManager.IsInRoleAsync(requestedUser, OmbiRoles.Admin))
            {
                sendNotification = false; // Don't bother sending a notification if the user is an admin
            }
            

            return new RuleResult
            {
                Success = sendNotification
            };
        }
    }
}