using Microsoft.Extensions.DependencyInjection;

namespace Ombi.Mapping
{
    public static class AutoMapperProfile
    {
        public static IServiceCollection AddOmbiMappingProfile(this IServiceCollection services)
        {
            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new OmbiProfile());
            });

            var mapper = config.CreateMapper();
            services.AddSingleton(mapper);

            return services;
        }
    }
}