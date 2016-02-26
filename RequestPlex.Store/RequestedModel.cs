using Dapper.Contrib.Extensions;

namespace RequestPlex.Store
{
    [Table("Requested")]
    public class RequestedModel : Entity
    {
        // ReSharper disable once IdentifierTypo
        public int Tmdbid { get; set; }
        public RequestType Type { get; set; }
    }

    public enum RequestType
    {
        Movie,
        TvShow
    }
}
