using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("PlexEpisode")]
    public class PlexEpisode : Entity, IMediaServerEpisode<PlexServerContent>
    {
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public int Key { get; set; } // RatingKey
        public string Title { get; set; }
        /// <summary>
        /// The Season key
        /// </summary>
        /// <value>
        /// The parent key.
        /// </value>
        public int ParentKey { get; set; }
        /// <summary>
        /// The Series key
        /// </summary>
        /// <value>
        /// The grandparent key.
        /// </value>
        public int GrandparentKey { get; set; }


        public PlexServerContent Series { get; set; }
    }
}