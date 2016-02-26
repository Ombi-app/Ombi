using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Data.Sqlite;

using RequestPlex.Store;

namespace RequestPlex.Core
{
    public class SettingsService
    {
        public void SaveSettings(int port)
        {
            var db = new DbConfiguration(new SqliteFactory());
            var repo = new GenericRepository<SettingsModel>(db);

            var existingSettings = repo.GetAll().FirstOrDefault();
            if (existingSettings != null)
            {
                existingSettings.Port = port;
                repo.Update(existingSettings);
                return;
            }

            var newSettings = new SettingsModel { Port = port };
            repo.Insert(newSettings);
        }

        public SettingsModel GetSettings()
        {
            var db = new DbConfiguration(new SqliteFactory());
            var repo = new GenericRepository<SettingsModel>(db);

            var settings = repo.GetAll().FirstOrDefault();

            return settings;
        }
    }
}
