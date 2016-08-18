#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: SettingsServiceV2.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System.Threading.Tasks;
using Newtonsoft.Json;

using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Store.Models;
using PlexRequests.Store.Repository;

namespace PlexRequests.Core
{
    public class SettingsServiceV2<T> : ISettingsService<T>
        where T : Settings, new()
    {

        public SettingsServiceV2(ISettingsRepository repo)
        {
            Repo = repo;
            EntityName = typeof(T).Name;
        }

        private ISettingsRepository Repo { get; }
        private string EntityName { get; }

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
            var result = await Repo.GetAsync(EntityName).ConfigureAwait(false);
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

                return insertResult != long.MinValue;
            }


            var modified = model;
            modified.Id = entity.Id;

            var globalSettings = new GlobalSettings { SettingsName = EntityName, Content = JsonConvert.SerializeObject(modified, SerializerSettings.Settings), Id = entity.Id };
            globalSettings.Content = EncryptSettings(globalSettings);
            var result = Repo.Update(globalSettings);

             return result;
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

                return insertResult != int.MinValue;
            }

            var modified = model;
            modified.Id = entity.Id;

            var globalSettings = new GlobalSettings { SettingsName = EntityName, Content = JsonConvert.SerializeObject(modified, SerializerSettings.Settings), Id = entity.Id };
            globalSettings.Content = EncryptSettings(globalSettings);
            var result = await Repo.UpdateAsync(globalSettings).ConfigureAwait(false);

            return result;
        }

        public bool Delete(T model)
        {
            var entity = Repo.Get(EntityName);
            if (entity != null)
            {
                return Repo.Delete(entity);
            }

            // Entity does not exist so nothing to delete
            return true;
        }

        public async Task<bool> DeleteAsync(T model)
        {
            var entity = Repo.Get(EntityName);
            if (entity != null)
            {
                return await Repo.DeleteAsync(entity);
            }
            
            return true;
        }

        private string EncryptSettings(GlobalSettings settings)
        {
            return StringCipher.Encrypt(settings.Content, settings.SettingsName);
        }

        private string DecryptSettings(GlobalSettings settings)
        {
            return StringCipher.Decrypt(settings.Content, settings.SettingsName);
        }
    }
}
