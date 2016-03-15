using System;
using System.Security.Cryptography;

using Dapper.Contrib.Extensions;

namespace PlexRequests.Store
{
    [Table("Requested")]
    public class RequestedModel : Entity
    {
        // ReSharper disable once IdentifierTypo
        public int ProviderId { get; set; }
        public string ImdbId { get; set; }
        public string Overview { get; set; }
        public string Title { get; set; }
        public string PosterPath { get; set; }
        public DateTime ReleaseDate { get; set; }
        public RequestType Type { get; set; }
        public string Status { get; set; }
        public bool Approved { get; set; }
        public string RequestedBy { get; set; }
        public DateTime RequestedDate { get; set; }
        public bool Available { get; set; }
        public IssueState Issues { get; set; }
        public string OtherMessage { get; set; }
        public bool LatestTv { get; set; }
        public string AdminNote { get; set; }
    }

    public enum RequestType
    {
        Movie,
        TvShow
    }

    public enum IssueState
    {
        None = 99,
        WrongAudio = 0,
        NoSubtitles = 1,
        WrongContent = 2,
        PlaybackIssues = 3,
        Other = 4 // Provide a message    
    }
}
