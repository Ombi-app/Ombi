using Ombi.Core.Models.Requests;
using Ombi.Core.Rule;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ombi.Core.Rules
{
    public class RuleEvaluator : IRuleEvaluator
    {
        public RuleEvaluator(IServiceProvider provider)
        {
            RequestRules = new List<IRequestRules<BaseRequestModel>>();
            var baseType = typeof(BaseRule).FullName;

            System.Reflection.Assembly ass = typeof(RuleEvaluator).GetTypeInfo().Assembly;

            foreach (TypeInfo ti in ass.DefinedTypes)
            {
                if (ti?.BaseType?.FullName == baseType)
                {
                    var type = ti.GetType();
                    var item = Activator.CreateInstance(ti.GetType(), provider.GetService(type));// ti.GetType is wrong
                    RequestRules.Add((IRequestRules<BaseRequestModel>)item);
                }
            }
        }

        private List<IRequestRules<BaseRequestModel>> RequestRules { get; }

        public IEnumerable<RuleResult> StartRequestRules(BaseRequestModel obj)
        {
            var results = new List<RuleResult>();
            foreach (var rule in RequestRules)
            {
                var result = rule.Execute(obj);
                results.Add(result);
            }

            return results;
        }
    }
}
