using Ombi.Core.Models.Requests;
using Ombi.Store.Repository.Requests;
using System.Collections.Generic;
using Ombi.Store.Entities;

namespace Ombi.Core.Models.Search
{
    public class SearchTvShowViewModel : SearchViewModel
    {
        public string Title { get; set; }
        public List<string> Aliases { get; set; }
        public string Banner { get; set; }
        public int SeriesId { get; set; }
        public string Status { get; set; }
        public string FirstAired { get; set; }
        public string Network { get; set; }
        public string NetworkId { get; set; }
        public string Runtime { get; set; }
        public List<string> Genre { get; set; }
        public string Overview { get; set; }
        public int LastUpdated { get; set; }
        public string AirsDayOfWeek { get; set; }
        public string AirsTime { get; set; }
        public string Rating { get; set; }
        public int SiteRating { get; set; }

        /// <summary>
        ///     This is used from the Trakt API
        /// </summary>
        /// <value>
        ///     The trailer.
        /// </value>
        public string Trailer { get; set; }

        /// <summary>
        ///     This is used from the Trakt API
        /// </summary>
        /// <value>
        ///     The trailer.
        /// </value>
        public string Homepage { get; set; }

        public List<SeasonRequests> SeasonRequests { get; set; } = new List<SeasonRequests>();

        /// <summary>
        ///     If we are requesting the entire series
        /// </summary>
        public bool RequestAll { get; set; }

        public bool FirstSeason { get; set; }
        public bool LatestSeason { get; set; }

        /// <summary>
        /// This is where we have EVERY Episode for that series
        /// </summary>
        public bool FullyAvailable { get; set; }
        // We only have some episodes
        public bool PartlyAvailable { get; set; }
        public override RequestType Type => RequestType.TvShow;

        /// <summary>
        /// Only set on the images call
        /// </summary>
        public string BackdropPath { get; set; }
    }
}