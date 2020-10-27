using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("ApplicationConfiguration")]
    public class ApplicationConfiguration : Entity
    {
        public ConfigurationTypes Type { get; set; }
        public string Value { get; set; }
    }

    public enum ConfigurationTypes
    {
        Url = 1,
        // 2 was used for Port before the beta
        FanartTv = 3,
        TheMovieDb = 4,
        StoragePath = 5,
        Notification = 6,
        BaseUrl = 7,
        SecurityToken = 8
    }
}