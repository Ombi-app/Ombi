using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Core.Models.Search
{
    public class SearchViewModel
    {
        public int Id { get; set; }
        public bool Approved { get; set; }
        public bool Requested { get; set; }
        public bool Available { get; set; }
        public string PlexUrl { get; set; }
        public string Quality { get; set; }


        /// <summary>
        /// This is used for the PlexAvailabilityCheck/EmbyAvailabilityRule rule
        /// </summary>
        /// <value>
        /// The custom identifier.
        /// </value>
        [NotMapped]
        public string CustomId { get; set; }
    }
}