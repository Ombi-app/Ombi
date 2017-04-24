using System.Security.Principal;
using Ombi.Core.Claims;
using Ombi.Core.Settings.Models;
using Ombi.Store.Entities;

namespace Ombi.Core.Engine.Interfaces
{
    public abstract class BaseEngine
    {
        protected BaseEngine(IPrincipal user)
        {
            User = user;
        }
        
        protected IPrincipal User { get; }

        protected string Username => User.Identity.Name;

        protected bool HasRole(string roleName)
        {
            return User.IsInRole(roleName);
        }

        protected bool ShouldSendNotification(RequestType type)
        {
            var sendNotification = !ShouldAutoApprove(type); /*|| !prSettings.IgnoreNotifyForAutoApprovedRequests;*/

            if (HasRole(OmbiClaims.Admin))
            {
                sendNotification = false; // Don't bother sending a notification if the user is an admin

            }
            return sendNotification;
        }

        public bool ShouldAutoApprove(RequestType requestType)
        {
            var admin = HasRole(OmbiClaims.Admin);
            // if the user is an admin, they go ahead and allow auto-approval
            if (admin) return true;

            // check by request type if the category requires approval or not
            switch (requestType)
            {
                case RequestType.Movie:
                    return HasRole(OmbiClaims.AutoApproveMovie);
                case RequestType.TvShow:
                    return HasRole(OmbiClaims.AutoApproveTv);
                default:
                    return false;
            }
        }


    }
}