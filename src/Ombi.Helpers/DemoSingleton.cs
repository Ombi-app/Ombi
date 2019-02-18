namespace Ombi.Helpers
{
    public class DemoSingleton
    {
        private static DemoSingleton instance;

        private DemoSingleton() { }

        public static DemoSingleton Instance => instance ?? (instance = new DemoSingleton());

        public bool Demo { get; set; }
    }
}