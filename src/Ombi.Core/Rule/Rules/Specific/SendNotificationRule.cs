using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Rule.Rules.Specific
{
    public class SendNotificationRule : SpecificRule, ISpecificRule<object>
    {
        public SendNotificationRule(OmbiUserManager um, ISettingsService<OmbiSettings> settings)
        {
            UserManager = um;
            Settings = settings;
        }

        public override SpecificRules Rule => SpecificRules.CanSendNotification;
        private OmbiUserManager UserManager { get; }
        private ISettingsService<OmbiSettings> Settings { get; }

        public async Task<RuleResult> Execute(object obj)
        {
            var req = (BaseRequest)obj;
            var settings = await Settings.GetSettingsAsync();
            var sendNotification = true;
            var requestedUser = await UserManager.Users.FirstOrDefaultAsync(x => x.Id == req.RequestedUserId);
            if (req.RequestType == RequestType.Movie)
            {
                if (settings.DoNotSendNotificationsForAutoApprove)
                {
                    sendNotification = !await UserManager.IsInRoleAsync(requestedUser, OmbiRoles.AutoApproveMovie);
                }
            }
            else if (req.RequestType == RequestType.TvShow)
            {
                if (settings.DoNotSendNotificationsForAutoApprove)
                {
                    sendNotification = !await UserManager.IsInRoleAsync(requestedUser, OmbiRoles.AutoApproveTv);
                }
            }
            else if (req.RequestType == RequestType.Album)
            {
                if (settings.DoNotSendNotificationsForAutoApprove)
                {
                    sendNotification = !await UserManager.IsInRoleAsync(requestedUser, OmbiRoles.AutoApproveMusic);
                }
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