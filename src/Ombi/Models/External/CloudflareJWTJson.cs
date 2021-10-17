using System;
using System.Collections.Generic;

namespace Ombi.Models.External
{
    public class CloudflareJWTJson
    {
        public IList<Dictionary<string, string>> keys { get; set; }
        public Dictionary<string, string> public_cert { get; set; }
        public IList<Dictionary<string, string>> public_certs { get; set; }
        public DateTime lastUpdate { get; set; }
    }
}