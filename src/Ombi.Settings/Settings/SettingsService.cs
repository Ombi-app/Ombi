using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Settings.Settings
{
    public class SettingsService<T> : ISettingsService<T>
        where T : Models.Settings, new()
    {

        public SettingsService(ISettingsRepository repo, ICacheService cache)
        {
            Repo = repo;
            EntityName = typeof(T).Name;
            _cache = cache;
        }

        private ISettingsRepository Repo { get; }
        private string EntityName { get; }
        private string CacheName => $"Settings{EntityName}";
        private readonly ICacheService _cache;

        public T GetSettings()
        {
            return _cache.GetOrAdd(CacheName, () =>
            {
                var result = Repo.Get(EntityName);
                if (result == null)
                {
                    return new T();
                }
                result.Content = DecryptSettings(result);
                var obj = string.IsNullOrEmpty(result.Content)
                    ? null
                    : JsonConvert.DeserializeObject<T>(result.Content, SerializerSettings.Settings);

                var model = obj;

                return model;
            }, DateTime.Now.AddHours(2));
        }

        public async Task<T> GetSettingsAsync()
        {
            return await _cache.GetOrAdd(CacheName, async () =>
            {
                var result = await Repo.GetAsync(EntityName);
                if (result == null)
                {
                    return new T();
                }
                result.Content = DecryptSettings(result);
                var obj = string.IsNullOrEmpty(result.Content)
                    ? null
                    : JsonConvert.DeserializeObject<T>(result.Content, SerializerSettings.Settings);

                var model = obj;

                return model;
            }, DateTime.Now.AddHours(5));
        }

        public bool SaveSettings(T model)
        {
            _cache.Remove(CacheName);
            var entity = Repo.Get(EntityName);

            if (entity == null)
            {
                var newEntity = model;

                var settings = new GlobalSettings { SettingsName = EntityName, Content = JsonConvert.SerializeObject(newEntity, SerializerSettings.Settings) };
                settings.Content = EncryptSettings(settings);
                var insertResult = Repo.Insert(settings);

                return insertResult != null;
            }


            var modified = model;
            modified.Id = entity.Id;
            entity.Content = JsonConvert.SerializeObject(modified, SerializerSettings.Settings);

            entity.Content = EncryptSettings(entity);
            Repo.Update(entity);

            return true;
        }

        public async Task<bool> SaveSettingsAsync(T model)
        {
            _cache.Remove(CacheName);
            var entity = await Repo.GetAsync(EntityName);

            if (entity == null)
            {
                var newEntity = model;

                var settings = new GlobalSettings { SettingsName = EntityName, Content = JsonConvert.SerializeObject(newEntity, SerializerSettings.Settings) };
                settings.Content = EncryptSettings(settings);
                var insertResult = await Repo.InsertAsync(settings);

                return insertResult != null;
            }

            var modified = model;
            modified.Id = entity.Id;

            entity.Content = JsonConvert.SerializeObject(modified, SerializerSettings.Settings);

            entity.Content = EncryptSettings(entity);
            await Repo.UpdateAsync(entity);

            return true;
        }

        public void Delete(T model)
        {
            _cache.Remove(CacheName);
            var entity = Repo.Get(EntityName);
            if (entity != null)
            {
                Repo.Delete(entity);
            }
        }

        public async Task DeleteAsync(T model)
        {
            _cache.Remove(CacheName);
            var entity = Repo.Get(EntityName);
            if (entity != null)
            {
                await Repo.DeleteAsync(entity);
            }

        }

        public void ClearCache()
        {
            _cache.Remove(CacheName);
        }

        private string EncryptSettings(GlobalSettings settings)
        {
            return settings.Content;
            //return _protector.Protect(settings.Content);
        }

        private string DecryptSettings(GlobalSettings settings)
        {
            return settings.Content;
            //return _protector.Unprotect(settings.Content);
        }
    }
}