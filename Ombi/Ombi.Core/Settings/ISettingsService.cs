using System.Threading.Tasks;

namespace Ombi.Core.Settings
{
    public interface ISettingsService<T>
    {
        T GetSettings();
        Task<T> GetSettingsAsync();
        bool SaveSettings(T model);
        Task<bool> SaveSettingsAsync(T model);
        bool Delete(T model);
        Task<bool> DeleteAsync(T model);
    }
}