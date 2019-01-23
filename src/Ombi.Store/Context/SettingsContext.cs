using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Ombi.Helpers;
using Ombi.Store.Entities;

namespace Ombi.Store.Context
{
    public sealed class SettingsContext : DbContext, ISettingsContext
    {
        private static bool _created;
        public SettingsContext()
        {
            if (_created) return;

            _created = true;
            Database.Migrate();
        }
        
        public DbSet<GlobalSettings> Settings { get; set; }
        public DbSet<ApplicationConfiguration> ApplicationConfigurations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var i = StoragePathSingleton.Instance;
            if (string.IsNullOrEmpty(i.StoragePath))
            {
                i.StoragePath = string.Empty;
            }
            optionsBuilder.UseSqlite($"Data Source={Path.Combine(i.StoragePath, "OmbiSettings.db")}");
        }

        public void Seed()
        {
            // Add the tokens
            var fanArt = ApplicationConfigurations.FirstOrDefault(x => x.Type == ConfigurationTypes.FanartTv);
            if (fanArt == null)
            {
                ApplicationConfigurations.Add(new ApplicationConfiguration
                {
                    Type = ConfigurationTypes.FanartTv,
                    Value = "4b6d983efa54d8f45c68432521335f15"
                });
                SaveChanges();
            }
            var movieDb = ApplicationConfigurations.FirstOrDefault(x => x.Type == ConfigurationTypes.FanartTv);
            if (movieDb == null)
            {
                ApplicationConfigurations.Add(new ApplicationConfiguration
                {
                    Type = ConfigurationTypes.TheMovieDb,
                    Value = "b8eabaf5608b88d0298aa189dd90bf00"
                });
                SaveChanges();
            }
            var notification = ApplicationConfigurations.FirstOrDefault(x => x.Type == ConfigurationTypes.Notification);
            if (notification == null)
            {
                ApplicationConfigurations.Add(new ApplicationConfiguration
                {
                    Type = ConfigurationTypes.Notification,
                    Value = "4f0260c4-9c3d-41ab-8d68-27cb5a593f0e"
                });
                SaveChanges();
            }

            SaveChanges();
        }

        ~SettingsContext()
        {

        }
    }
}