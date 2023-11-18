using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Ombi.Store.Entities
{
    [Table(nameof(UserQualityProfiles))]
    public class UserQualityProfiles : Entity
    {
        public string UserId { get; set; }

        public int SonarrQualityProfileAnime { get; set; }
        public int SonarrRootPathAnime { get; set; }
        public int SonarrRootPath { get; set; }
        public int SonarrQualityProfile { get; set; }
        public int RadarrRootPath { get; set; }
        public int RadarrQualityProfile { get; set; }
        public int Radarr4KRootPath { get; set; }
        public int Radarr4KQualityProfile { get; set; }

        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public OmbiUser User { get; set; }
    }
}
