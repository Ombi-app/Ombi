using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Ombi.Store.Entities
{
    [Table(nameof(UserSelectableQualityProfile))]
    public class UserSelectableQualityProfile : Entity
    {
        public string UserId { get; set; }
        public SelectableQualityProfileApplication Application { get; set; }
        public int QualityProfileId { get; set; }
        public bool Is4K { get; set; }

        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public OmbiUser User { get; set; }
    }

    public enum SelectableQualityProfileApplication
    {
        Radarr = 0,
        Sonarr = 1
    }
}
