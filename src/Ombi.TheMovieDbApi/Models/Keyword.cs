using System.Runtime.Serialization;

namespace Ombi.Api.TheMovieDb.Models
{
    public sealed class Keyword
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
