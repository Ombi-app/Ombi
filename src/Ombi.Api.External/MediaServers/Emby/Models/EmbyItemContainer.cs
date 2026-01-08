using System.Collections.Generic;

namespace Ombi.Api.External.MediaServers.Emby.Models
{
    public class EmbyItemContainer<T>
    {
        public List<T> Items { get; set; }
        public int TotalRecordCount { get; set; }
    }
}