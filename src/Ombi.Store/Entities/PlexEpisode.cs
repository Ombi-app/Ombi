using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("PlexEpisode")]
    public class PlexEpisode : Entity
    {
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string Key { get; set; } // RatingKey
        public string Title { get; set; }
        /// <summary>
        /// The Show key
        /// </summary>
        /// <value>
        /// The parent key.
        /// </value>
        public string ParentKey { get; set; }
        /// <summary>
        /// The Series key
        /// </summary>
        /// <value>
        /// The grandparent key.
        /// </value>
        public string GrandparentKey { get; set; }
    }
}