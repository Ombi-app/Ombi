using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ombi.Mapping
{
    public static class AutoMapperProfile
    {
        public static IServiceCollection AddOmbiMappingProfile(this IServiceCollection services)
        {
            System.Reflection.Assembly ass = typeof(AutoMapperProfile).GetTypeInfo().Assembly;
            var assemblies = new List<Type>();
            foreach (System.Reflection.TypeInfo ti in ass.DefinedTypes)
            {
                if (ti.ImplementedInterfaces.Contains(typeof(IProfileConfiguration)))
                {
                    assemblies.Add(ti.AsType());
                }
            }
            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                //cfg.AddProfile(new OmbiProfile());
                cfg.AddProfiles(assemblies);
            });

            var mapper = config.CreateMapper();
            services.AddSingleton(mapper);

            return services;
        }
    }
}