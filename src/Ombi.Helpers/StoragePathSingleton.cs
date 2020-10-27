namespace Ombi.Helpers
{
    public class StoragePathSingleton
    {
        private static StoragePathSingleton instance;

        private StoragePathSingleton() { }

        public static StoragePathSingleton Instance => instance ?? (instance = new StoragePathSingleton());

        public string StoragePath { get; set; }
        public string SecurityToken { get; set; }
    }
}