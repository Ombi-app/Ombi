using Ombi.Core.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Helpers;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Rule
{
    public class RuleEvaluator : IRuleEvaluator
    {
        public RuleEvaluator(IServiceProvider provider)
        {
            RequestRules = new List<IRules<BaseRequest>>();
            SearchRules = new List<IRules<SearchViewModel>>();
            SpecificRules = new List<ISpecificRule<object>>();

            var ass = typeof(RuleEvaluator).GetTypeInfo().Assembly;

            foreach (var instance in CreateInstances(provider, ass, typeof(BaseRequestRule).FullName))
            {
                RequestRules.Add((IRules<BaseRequest>)instance);
            }
            foreach (var instance in CreateInstances(provider, ass, typeof(BaseSearchRule).FullName))
            {
                SearchRules.Add((IRules<SearchViewModel>)instance);
            }
            foreach (var instance in CreateInstances(provider, ass, typeof(SpecificRule).FullName))
            {
                SpecificRules.Add((ISpecificRule<object>)instance);
            }
        }

        private List<IRules<BaseRequest>> RequestRules { get; }
        private List<IRules<SearchViewModel>> SearchRules { get; }
        private List<ISpecificRule<object>> SpecificRules { get; }

        public async Task<IEnumerable<RuleResult>> StartRequestRules(BaseRequest obj)
        {
            var results = new List<RuleResult>();
            foreach (var rule in RequestRules)
            {
                var result = await rule.Execute(obj);
                results.Add(result);
            }

            return results;
        }

        public async Task<IEnumerable<RuleResult>> StartSearchRules(SearchViewModel obj)
        {
            var results = new List<RuleResult>();
            foreach (var rule in SearchRules)
            {
                var result = await rule.Execute(obj);
                results.Add(result);
            }

            return results;
        }

        public async Task<RuleResult> StartSpecificRules(object obj, SpecificRules selectedRule, string requestOnBehalf)
        {
            foreach (var rule in SpecificRules)
            {
                if (selectedRule == rule.Rule)
                {
                    var result = await rule.Execute(obj, requestOnBehalf);
                    return result;
                }
            }

            throw new RuleNotFoundException(nameof(selectedRule));
        }

        private static bool InheritsFrom(TypeInfo ti, string baseTypeName)
        {
            var current = ti?.BaseType;
            while (current != null)
            {
                if (current.FullName == baseTypeName)
                {
                    return true;
                }
                current = current.BaseType;
            }
            return false;
        }

        private static IEnumerable<object> CreateInstances(IServiceProvider provider, Assembly ass, string baseTypeName)
        {
            foreach (var ti in ass.DefinedTypes)
            {
                if (ti.IsAbstract || !InheritsFrom(ti, baseTypeName))
                {
                    continue;
                }

                var type = ti.AsType();
                var ctor = type.GetConstructors().FirstOrDefault();
                if (ctor == null)
                {
                    continue;
                }

                var services = ctor.GetParameters()
                    .Select(p => provider.GetService(p.ParameterType))
                    .ToArray();

                yield return Activator.CreateInstance(type, services);
            }
        }
    }
}
