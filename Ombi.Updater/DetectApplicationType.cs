using Ombi.Common;
using Ombi.Common.EnvironmentInfo;
using Ombi.Common.Processes;

namespace Ombi.Updater
{
    public class DetectApplicationType
    {
        public DetectApplicationType()
        {
            _processProvider = new ProcessProvider();
            _serviceProvider = new ServiceProvider(_processProvider);
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly IProcessProvider _processProvider;
        public AppType GetAppType()
        {
            if (OsInfo.IsNotWindows)
            {
                // Technically it is the console, but it has been renamed for mono (Linux/OS X)
                return AppType.Normal;
            }

            if (_serviceProvider.ServiceExist(ServiceProvider.OmbiServiceName))
            {
                return AppType.Service;
            }

            if (_processProvider.Exists(ProcessProvider.OmbiProcessName))
            {
                return AppType.Console;
            }

            return AppType.Normal;
        }
    }
}
