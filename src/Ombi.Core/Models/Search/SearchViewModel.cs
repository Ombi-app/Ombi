using System.ComponentModel.DataAnnotations.Schema;
using Ombi.Store.Entities;

namespace Ombi.Core.Models.Search
{
    public abstract class SearchViewModel
    {
        public int Id { get; set; }
        public bool Approved { get; set; }
        public bool? Denied { get; set; }
        public string DeniedReason { get; set; }
        public bool Requested { get; set; }
        public int RequestId { get; set; }
        public bool Available { get; set; }
        public string PlexUrl { get; set; }
        public string EmbyUrl { get; set; }
        public string JellyfinUrl { get; set; }
        public string Quality { get; set; }
        public abstract RequestType Type { get; }

        /// <summary>
        /// This is used for the PlexAvailabilityCheck/EmbyAvailabilityRule/JellyfinAvailabilityRule rule
        /// </summary>
        /// <value>
        /// The custom identifier.
        /// </value>
        [NotMapped]
        public string ImdbId { get; set; }
        [NotMapped]
        public string TheTvDbId { get; set; }
        [NotMapped]
        public string TheMovieDbId { get; set; }

        [NotMapped]
        public bool Subscribed { get; set; }
        [NotMapped]
        public bool ShowSubscribe { get; set; }
    }
}
