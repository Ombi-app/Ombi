using Ombi.Core.Models.Requests;
using Ombi.Core.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ombi.Core.Rule
{
    public class RuleEvaluator : IRuleEvaluator
    {
        public RuleEvaluator(IServiceProvider provider)
        {
            RequestRules = new List<IRequestRules<BaseRequestModel>>();
            var baseType = typeof(BaseRule).FullName;

            var ass = typeof(RuleEvaluator).GetTypeInfo().Assembly;

            foreach (var ti in ass.DefinedTypes)
                if (ti?.BaseType?.FullName == baseType)
                {
                    var type = ti?.AsType();
                    var ctors = type.GetConstructors();
                    var ctor = ctors.FirstOrDefault();

                    var services = new List<object>();
                    foreach (var param in ctor.GetParameters())
                        services.Add(provider.GetService(param.ParameterType));

                    var item = Activator.CreateInstance(type, services.ToArray()); // ti.GetType is wrong
                    RequestRules.Add((IRequestRules<BaseRequestModel>) item);
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