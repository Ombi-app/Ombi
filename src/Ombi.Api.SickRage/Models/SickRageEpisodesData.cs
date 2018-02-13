using System.Collections.Generic;

namespace Ombi.Api.SickRage.Models
{
    public class SickRageEpisodes : SickRageBase<Dictionary<int, SickRageEpisodesData>>
    {

    }

    public class SickRageEpisodesData
    {
        public string airdate { get; set; }
        public string name { get; set; }
        public string quality { get; set; }
        public string status { get; set; }
    }

}