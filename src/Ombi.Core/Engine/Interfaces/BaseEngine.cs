using Ombi.Core.Claims;
using Ombi.Core.Models.Requests;
using Ombi.Core.Rule;
using Ombi.Core.Rules;
using Ombi.Store.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Ombi.Core.Models.Search;

namespace Ombi.Core.Engine.Interfaces
{
    public abstract class BaseEngine
    {
        protected BaseEngine(IPrincipal user, IRuleEvaluator rules)
        {
            User = user;
            Rules = rules;
        }

        protected IPrincipal User { get; }

        protected IRuleEvaluator Rules { get; }

        protected string Username => User.Identity.Name;

        protected bool HasRole(string roleName)
        {
            return User.IsInRole(roleName);
        }

        protected bool ShouldSendNotification(RequestType type)
        {
            var sendNotification = !ShouldAutoApprove(type); /*|| !prSettings.IgnoreNotifyForAutoApprovedRequests;*/

            if (HasRole(OmbiClaims.Admin))
                sendNotification = false; // Don't bother sending a notification if the user is an admin
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

        public IEnumerable<RuleResult> RunRequestRules(BaseRequestModel model)
        {
            var ruleResults = Rules.StartRequestRules(model).ToList();
            return ruleResults;
        }

        public IEnumerable<RuleResult> RunSearchRules(SearchViewModel model)
        {
            var ruleResults = Rules.StartSearchRules(model).ToList();
            return ruleResults;
        }
    }
}