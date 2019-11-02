using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ombi.Helpers;
using Ombi.Store.Context;

namespace Ombi
{
    public static class DatabaseExtensions
    {
        public static void ConfigureDatabases(this IServiceCollection services)
        {
            services.AddDbContext<OmbiContext, OmbiSqliteContext>(ConfigureSqlite);
        }

        private static void ConfigureSqlite(DbContextOptionsBuilder options)
        {
            var i = StoragePathSingleton.Instance;
            if (string.IsNullOrEmpty(i.StoragePath))
            {
                i.StoragePath = string.Empty;
            }
            options.UseSqlite($"Data Source={Path.Combine(i.StoragePath, "Ombi.db")}");
        }
    }
}