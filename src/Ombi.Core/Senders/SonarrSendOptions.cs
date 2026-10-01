using Ombi.Api.External.ExternalApis.Sonarr.Models;
using System.Collections.Generic;

namespace Ombi.Core.Senders
{
    internal class SonarrSendOptions
    {
        public List<int> Tags { get; set; } = new List<int>();
        public Dictionary<int, int> IdentityRepairSeasonNumberMap { get; } = new Dictionary<int, int>();
    }
}
