using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ombi.Settings.Settings.Models
{
    public class FeatureSettings : Settings
    {
        public List<FeatureEnablement> Features { get; set; }
    }

    public class FeatureEnablement
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }

    public static class FeatureNames
    {
        public const string Movie4KRequests = nameof(Movie4KRequests);
        public const string OldTrendingSource = nameof(OldTrendingSource);
    }
}
