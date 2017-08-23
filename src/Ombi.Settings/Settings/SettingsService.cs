using System.Threading.Tasks;
using Newtonsoft.Json;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Microsoft.AspNetCore.DataProtection;

namespace Ombi.Settings.Settings
{
    public class SettingsService<T> : ISettingsService<T>
        where T : Ombi.Settings.Settings.Models.Settings, new()
    {

        public SettingsService(ISettingsRepository repo, IDataProtectionProvider provider)
        {
            Repo = repo;
            EntityName = typeof(T).Name;
            _protector = provider.CreateProtector(GetType().FullName);
        }

        private ISettingsRepository Repo { get; }
        private string EntityName { get; }
        private readonly IDataProtector _protector;

        public T GetSettings()
        {
            var result = Repo.Get(EntityName);
            if (result == null)
            {
                return new T();
            }
            result.Content = DecryptSettings(result);
            var obj = string.IsNullOrEmpty(result.Content) ? null : JsonConvert.DeserializeObject<T>(result.Content, SerializerSettings.Settings);

            var model = obj;

            return model;
        }

        public async Task<T> GetSettingsAsync()
        {
            var result = await Repo.GetAsync(EntityName);
            if (result == null)
            {
                return new T();
            }
            result.Content = DecryptSettings(result);
            return string.IsNullOrEmpty(result.Content) ? null : JsonConvert.DeserializeObject<T>(result.Content, SerializerSettings.Settings);
        }

        public bool SaveSettings(T model)
        {
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

            var globalSettings = new GlobalSettings { SettingsName = EntityName, Content = JsonConvert.SerializeObject(modified, SerializerSettings.Settings), Id = entity.Id };
            globalSettings.Content = EncryptSettings(globalSettings);
            Repo.Update(globalSettings);

            return true;
        }

        public async Task<bool> SaveSettingsAsync(T model)
        {
            var entity = await Repo.GetAsync(EntityName);

            if (entity == null)
            {
                var newEntity = model;

                var settings = new GlobalSettings { SettingsName = EntityName, Content = JsonConvert.SerializeObject(newEntity, SerializerSettings.Settings) };
                settings.Content = EncryptSettings(settings);
                var insertResult = await Repo.InsertAsync(settings).ConfigureAwait(false);

                return insertResult != null;
            }

            var modified = model;
            modified.Id = entity.Id;

            var globalSettings = new GlobalSettings { SettingsName = EntityName, Content = JsonConvert.SerializeObject(modified, SerializerSettings.Settings), Id = entity.Id };
            globalSettings.Content = EncryptSettings(globalSettings);
            await Repo.UpdateAsync(globalSettings).ConfigureAwait(false);

            return true;
        }

        public void Delete(T model)
        {
            var entity = Repo.Get(EntityName);
            if (entity != null)
            {
                Repo.Delete(entity);
            }

        }

        public async Task DeleteAsync(T model)
        {
            var entity = Repo.Get(EntityName);
            if (entity != null)
            {
                await Repo.DeleteAsync(entity);
            }

        }

        private string EncryptSettings(GlobalSettings settings)
        {
            return _protector.Protect(settings.Content);
        }

        private string DecryptSettings(GlobalSettings settings)
        {
            return _protector.Unprotect(settings.Content);
        }
    }
}