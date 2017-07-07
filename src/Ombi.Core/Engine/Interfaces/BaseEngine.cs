using Ombi.Core.Rule;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities.Requests;

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
        public async Task<RuleResult> RunSpecificRule(object model, SpecificRules rule)
        {
            var ruleResults = await Rules.StartSpecificRules(model, rule);
            return ruleResults;
        }
    }
}