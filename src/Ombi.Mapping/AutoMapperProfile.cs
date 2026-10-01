using System.Collections.Generic;
using AutoMapper;
using AutoMapper.EquivalencyExpression;
using Microsoft.Extensions.DependencyInjection;
using Ombi.Mapping.Profiles;

namespace Ombi.Mapping
{
    public static class AutoMapperProfile
    {
        public static IServiceCollection AddOmbiMappingProfile(this IServiceCollection services)
        {
            var profiles = new List<Profile>
            {
                new MovieProfile(),
                new OmbiProfile(),
                new SettingsProfile(),
                new TvProfile(),
                new TvProfileV2()
            };
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfiles(profiles);
                cfg.AddCollectionMappers();
            });

            return services;
        }
    }
}