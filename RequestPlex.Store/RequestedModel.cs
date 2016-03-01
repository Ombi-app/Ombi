using System;

using Dapper.Contrib.Extensions;

namespace RequestPlex.Store
{
    [Table("Requested")]
    public class RequestedModel : Entity
    {
        // ReSharper disable once IdentifierTypo
        public int Tmdbid { get; set; }
        public string ImdbId { get; set; }
        public string Overview { get; set; }
        public string Title { get; set; }
        public string PosterPath { get; set; }
        public DateTime ReleaseDate { get; set; }
        public RequestType Type { get; set; }
        public string Status { get; set; }
    }

    public enum RequestType
    {
        Movie,
        TvShow
    }
}
