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

        public void AddRequest(int tmdbid, RequestType type)
        {
            var model = new RequestedModel
            {
                Tmdbid = tmdbid,
                Type = type
            };

            var db = new DbConfiguration(new SqliteFactory());
            var repo = new GenericRepository<RequestedModel>(db);

            repo.Insert(model);
        }

        public bool CheckRequest(int tmdbid)
        {
            var db = new DbConfiguration(new SqliteFactory());
            var repo = new GenericRepository<RequestedModel>(db);

            return repo.GetAll().Any(x => x.Tmdbid == tmdbid);
        }

        public void DeleteRequest(int tmdbId)
        {
            var db = new DbConfiguration(new SqliteFactory());
            var repo = new GenericRepository<RequestedModel>(db);
            var entity = repo.GetAll().FirstOrDefault(x => x.Tmdbid == tmdbId);
            repo.Delete(entity);
        }

    }
}
