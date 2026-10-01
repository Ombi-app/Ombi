using AutoMapper.EquivalencyExpression;
using Microsoft.Extensions.DependencyInjection;

namespace Ombi.Mapping
{
    public static class AutoMapperProfile
    {
        public static IServiceCollection AddOmbiMappingProfile(this IServiceCollection services)
        {
            services.AddAutoMapper(
                cfg => cfg.AddCollectionMappers(),
                typeof(AutoMapperProfile));

            return services;
        }
    }
}
