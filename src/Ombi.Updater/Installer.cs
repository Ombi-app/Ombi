using System;
using System.Collections.Generic;
using System.Text;

namespace Ombi.Updater
{
    public class Installer
    {
        public void Start(StartupOptions options)
        {
            // Kill Ombi Process
            var p = new ProcessProvider();

            p.Kill(options.OmbiProcessId);

        }

        
    }
}
