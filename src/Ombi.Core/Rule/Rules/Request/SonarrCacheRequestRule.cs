using System.Threading.Tasks;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Context;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Rule.Rules.Request
{
    public class SonarrCacheRequestRule : BaseRequestRule, IRules<BaseRequest>
    {
        public SonarrCacheRequestRule(ExternalContext ctx)
        {
            _ctx = ctx;
        }

        private readonly ExternalContext _ctx;

        public Task<RuleResult> Execute(BaseRequest obj)
        {
            var rule = new SonarrCacheRule(_ctx);
            return rule.Execute(obj);
        }
    }
}