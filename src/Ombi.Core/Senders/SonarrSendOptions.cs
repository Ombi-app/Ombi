using Ombi.Api.Sonarr.Models;
using System.Collections.Generic;

namespace Ombi.Core.Senders
{
    internal class SonarrSendOptions
    {
        public List<int> Tags { get; set; } = new List<int>();
    }
}
