using System.Collections.Generic;

namespace Ombi.Api.External.ExternalApis.SickRage.Models
{
    public class SickRageShows : SickRageBase<Dictionary<int, Item>>
    {

    }

    public class Item
    {
        public int tvdbid { get; set; }
    }
}