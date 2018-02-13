namespace Ombi.Core.Settings
{
    public interface ISettingsResolver
    {
        ISettingsService<T> Resolve<T>();
    }
}