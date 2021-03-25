using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ombi.Core.Models.Requests
{
    public class TvRequestViewModel : TvRequestViewModelBase
    {
        public int TvDbId { get; set; }
    }

    public class SeasonsViewModel
    {
        public int SeasonNumber { get; set; }
        public List<EpisodesViewModel> Episodes { get; set; } = new List<EpisodesViewModel>();
    }

    public class EpisodesViewModel
    {
        public int EpisodeNumber { get; set; }
    }


    public class TvRequestViewModelBase : BaseRequestOptions
    {
        public bool RequestAll { get; set; }
        public bool LatestSeason { get; set; }
        public bool FirstSeason { get; set; }
        public List<SeasonsViewModel> Seasons { get; set; } = new List<SeasonsViewModel>();
        [JsonIgnore]
        public string RequestedByAlias { get; set; }
    }
}