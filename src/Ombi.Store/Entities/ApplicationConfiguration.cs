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
        Url,
        Port,
        FanartTv,
        TheMovieDb
    }
}