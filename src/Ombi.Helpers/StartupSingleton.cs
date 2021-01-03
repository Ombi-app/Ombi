namespace Ombi.Helpers
{
    public class StartupSingleton
    {
        private static StartupSingleton instance;

        private StartupSingleton() { }

        public static StartupSingleton Instance => instance ?? (instance = new StartupSingleton());

        public string StoragePath { get; set; }

        public string SecurityKey { get; set; }
#if DEBUG
        = "test";
#endif
    }
}