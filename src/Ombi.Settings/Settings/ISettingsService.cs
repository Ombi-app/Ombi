using System;
using System.Threading.Tasks;

namespace Ombi.Core.Settings
{
    public interface ISettingsService<T>
    {
        T GetSettings();
        Task<T> GetSettingsAsync();
        bool SaveSettings(T model);
        Task<bool> SaveSettingsAsync(T model);
        void Delete(T model);
        Task DeleteAsync(T model);
        void ClearCache();
    }
}