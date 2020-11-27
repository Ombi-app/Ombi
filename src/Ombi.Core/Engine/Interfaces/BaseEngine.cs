using System;
using Ombi.Core.Rule;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Entities;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;

namespace Ombi.Core.Engine.Interfaces
{
    public abstract class BaseEngine
    {
        protected BaseEngine(IPrincipal user, OmbiUserManager um, IRuleEvaluator rules)
        {
            UserPrinciple = user;
            Rules = rules;
            UserManager = um;
        }

        protected IPrincipal UserPrinciple { get; }
        protected IRuleEvaluator Rules { get; }
        protected OmbiUserManager UserManager { get;  }
        protected string Username => UserPrinciple.Identity.Name;

        private OmbiUser _user;
        protected async Task<OmbiUser> GetUser()
        {
            var username = Username.ToUpper();
            return _user ?? (_user = await UserManager.Users.FirstOrDefaultAsync(x => x.NormalizedUserName == username));
        }

        protected async Task<string> UserAlias()
        {
            return (await GetUser()).UserAlias;
        }

        protected async Task<bool> IsInRole(string roleName)
        {
            return await UserManager.IsInRoleAsync(await GetUser(), roleName);
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