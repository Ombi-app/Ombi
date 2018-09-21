using System;

namespace Ombi.Api.Lidarr.Models
{
    public class LidarrStatus
    {
        public string version { get; set; }
        public DateTime buildTime { get; set; }
        public bool isDebug { get; set; }
        public bool isProduction { get; set; }
        public bool isAdmin { get; set; }
        public bool isUserInteractive { get; set; }
        public string startupPath { get; set; }
        public string appData { get; set; }
        public string osName { get; set; }
        public string osVersion { get; set; }
        public bool isMonoRuntime { get; set; }
        public bool isMono { get; set; }
        public bool isLinux { get; set; }
        public bool isOsx { get; set; }
        public bool isWindows { get; set; }
        public string mode { get; set; }
        public string branch { get; set; }
        public string authentication { get; set; }
        public string sqliteVersion { get; set; }
        public int migrationVersion { get; set; }
        public string urlBase { get; set; }
        public string runtimeVersion { get; set; }
        public string runtimeName { get; set; }
    }
}