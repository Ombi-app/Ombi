using Ombi.Core.Models.Requests;
using Ombi.Core.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;

namespace Ombi.Core.Rule
{
    public class RuleEvaluator : IRuleEvaluator
    {
        public RuleEvaluator(IServiceProvider provider)
        {
            RequestRules = new List<IRequestRules<BaseRequestModel>>();
            SearchRules = new List<IRequestRules<SearchViewModel>>();
            var baseSearchType = typeof(BaseSearchRule).FullName;
            var baseRequestType = typeof(BaseRequestRule).FullName;

            var ass = typeof(RuleEvaluator).GetTypeInfo().Assembly;

            foreach (var ti in ass.DefinedTypes)
            {
                if (ti?.BaseType?.FullName == baseSearchType)
                {
                    var type = ti?.AsType();
                    var ctors = type.GetConstructors();
                    var ctor = ctors.FirstOrDefault();

                    var services = new List<object>();
                    foreach (var param in ctor.GetParameters())
                    {
                        services.Add(provider.GetService(param.ParameterType));
                    }

                    var item = Activator.CreateInstance(type, services.ToArray());
                    RequestRules.Add((IRequestRules<BaseRequestModel>) item);
                }
            }
            
            foreach (var ti in ass.DefinedTypes)
            {
                if (ti?.BaseType?.FullName == baseRequestType)
                {
                    var type = ti?.AsType();
                    var ctors = type.GetConstructors();
                    var ctor = ctors.FirstOrDefault();

                    var services = new List<object>();
                    foreach (var param in ctor.GetParameters())
                    {
                        services.Add(provider.GetService(param.ParameterType));
                    }

                    var item = Activator.CreateInstance(type, services.ToArray());
                    SearchRules.Add((IRequestRules<SearchViewModel>) item);
                }
            }
        }

        private List<IRequestRules<BaseRequestModel>> RequestRules { get; }
        private List<IRequestRules<SearchViewModel>> SearchRules { get; }

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

        public IEnumerable<RuleResult> StartSearchRules(SearchViewModel obj)
        {
            var results = new List<RuleResult>();
            foreach (var rule in SearchRules)
            {
                var result = rule.Execute(obj);
                results.Add(result);
            }

            return results;
        }
    }
}