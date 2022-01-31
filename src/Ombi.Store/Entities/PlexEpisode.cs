using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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

        public override IMediaServerContent SeriesIsIn(ICollection<IMediaServerContent> content)
        {
            return content.OfType<PlexServerContent>().FirstOrDefault(
                 x => x.Key == this.PlexSeries.Key);
        }

        public override bool IsIn(IMediaServerContent content)
        {
            return content.Episodes.Cast<PlexEpisode>().Any(x => x.Key == this.Key);
        }

    }
}