using System;
using Ombi.Core.Rule;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Entities;
using Ombi.Core.Authentication;
using Ombi.Core.Helpers;

namespace Ombi.Core.Engine.Interfaces
{
    public abstract class BaseEngine
    {
        protected BaseEngine(ICurrentUser user, OmbiUserManager um, IRuleEvaluator rules)
        {
            CurrentUser = user;
            Rules = rules;
            UserManager = um;
        }

        protected ICurrentUser CurrentUser { get; }
        protected IRuleEvaluator Rules { get; }
        protected OmbiUserManager UserManager { get; }
        protected string Username => CurrentUser.Username;
        protected Task<OmbiUser> GetUser() => CurrentUser.GetUser();

        /// <summary>
        /// Only used for background tasks
        /// </summary>
        public void SetUser(OmbiUser user) => CurrentUser.SetUser(user);

        protected async Task<string> UserAlias()
        {
            return (await GetUser()).UserAlias;
        }

        protected async Task<bool> IsInRole(string roleName)
        {
            if (Username.Equals("API", StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            var user = await GetUser();
            return await UserManager.IsInRoleAsync(user, roleName);
        }

        public async Task<IEnumerable<RuleResult>> RunRequestRules(BaseRequest model)
        {
            var ruleResults = await Rules.StartRequestRules(model);
            return ruleResults;
        }

        public async Task<IEnumerable<RuleResult>> RunSearchRules(SearchViewModel model)
        {
            var ruleResults = await Rules.StartSearchRules(model);
            return ruleResults;
        }
        public async Task<RuleResult> RunSpecificRule(object model, SpecificRules rule, string requestOnBehalf)
        {
            var ruleResults = await Rules.StartSpecificRules(model, rule, requestOnBehalf);
            return ruleResults;
        }
    }
}