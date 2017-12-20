using Newtonsoft.Json;
using Ombi.Helpers;

namespace Ombi.Settings.Settings.Models.External
{
    public abstract class ExternalSettings : Models.Settings
    {
        public bool Ssl { get; set; }
        public string SubDir { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }

        [JsonIgnore]
        public virtual string FullUri
        {
            get
            {
                if (!string.IsNullOrEmpty(SubDir))
                {
                    var formattedSubDir = Ip.ReturnUriWithSubDir(Port, Ssl, SubDir.Trim());
                    return formattedSubDir.ToString();
                }
                var formatted = Ip.ReturnUri(Port, Ssl);
                return formatted.ToString();
            }
        }
    }
}