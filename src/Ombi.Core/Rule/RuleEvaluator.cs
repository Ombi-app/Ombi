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
            
            var baseSearchType = typeof(BaseRequestRule).FullName;
            var baseRequestType = typeof(BaseSearchRule).FullName;
            var baseSpecificRuleType = typeof(SpecificRule).FullName;

            var ass = typeof(RuleEvaluator).GetTypeInfo().Assembly;

            GetTypes(provider, ass, baseSearchType, RequestRules);
            GetTypes(provider, ass, baseRequestType, SearchRules);
            GetTypes(provider, ass, baseSpecificRuleType, SpecificRules);
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


        private void GetTypes<T>(IServiceProvider provider, Assembly ass, string baseSearchType, List<IRules<T>> ruleList)
        {
            foreach (var ti in ass.DefinedTypes)
            {
                if (ti?.BaseType?.FullName == baseSearchType)
                {
                    var type = ti?.AsType();
                    var ctors = type.GetConstructors();
                    var ctor = ctors.FirstOrDefault();

                    var services = new List<object>();
                    foreach (var param in ctor?.GetParameters())
                    {
                        services.Add(provider.GetService(param.ParameterType));
                    }

                    var item = Activator.CreateInstance(type, services.ToArray());
                    ruleList.Add((IRules<T>)item);
                }
            }
        }

        private void GetTypes<T>(IServiceProvider provider, Assembly ass, string baseSearchType, ICollection<ISpecificRule<T>> ruleList) where T : new()
        {
            foreach (var ti in ass.DefinedTypes)
            {
                if (ti?.BaseType?.FullName == baseSearchType)
                {
                    var type = ti?.AsType();
                    var ctors = type.GetConstructors();
                    var ctor = ctors.FirstOrDefault();

                    var services = new List<object>();
                    foreach (var param in ctor?.GetParameters())
                    {
                        services.Add(provider.GetService(param.ParameterType));
                    }

                    var item = Activator.CreateInstance(type, services.ToArray());
                    ruleList.Add((ISpecificRule<T>)item);
                }
            }
        }
    }
}