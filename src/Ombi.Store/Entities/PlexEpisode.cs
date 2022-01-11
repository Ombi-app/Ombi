using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("PlexEpisode")]
    public class PlexEpisode : MediaServerEpisode
    {
        public int Key { get; set; } // RatingKey
        /// <value>
        /// The parent key.
        /// </value>
        public int ParentKey { get; set; }
        /// <value>
        /// The grandparent key.
        /// </value>
        public int GrandparentKey { get; set; }
        [NotMapped]
        public PlexServerContent PlexSeries
        {
            get => (PlexServerContent)Series;
            set => Series = value;
        }
    }
}