using System;
using Newtonsoft.Json;
using Ombi.Helpers;

namespace Ombi.Core.Settings.Models.External
{
    public abstract class ExternalSettings : Settings
    {
        public bool Ssl { get; set; }
        public string SubDir { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }

        [JsonIgnore]
        public virtual Uri FullUri
        {
            get
            {
                if (!string.IsNullOrEmpty(SubDir))
                {
                    var formattedSubDir = Ip.ReturnUriWithSubDir(Port, Ssl, SubDir);
                    return formattedSubDir;
                }
                var formatted = Ip.ReturnUri(Port, Ssl);
                return formatted;
            }
        }
    }
}