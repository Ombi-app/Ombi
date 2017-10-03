using System.ComponentModel.DataAnnotations.Schema;
using Ombi.Store.Entities;

namespace Ombi.Core.Models.Search
{
    public abstract class SearchViewModel
    {
        public int Id { get; set; }
        public bool Approved { get; set; }
        public bool Requested { get; set; }
        public bool Available { get; set; }
        public string PlexUrl { get; set; }
        public string Quality { get; set; }
        public abstract RequestType Type { get; }


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