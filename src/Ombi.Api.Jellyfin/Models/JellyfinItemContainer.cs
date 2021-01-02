using System.Collections.Generic;

namespace Ombi.Api.Jellyfin.Models
{
    public class JellyfinItemContainer<T>
    {
        public List<T> Items { get; set; }
        public int TotalRecordCount { get; set; }
    }
}