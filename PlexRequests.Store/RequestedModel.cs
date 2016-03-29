using System;
using System.Security.Cryptography;

using Dapper.Contrib.Extensions;
using System.Collections.Generic;
using System.Linq;

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

        [Obsolete("Use RequestedUsers")]
        public string RequestedBy { get; set; }

        public DateTime RequestedDate { get; set; }
        public bool Available { get; set; }
        public IssueState Issues { get; set; }
        public string OtherMessage { get; set; }
        public string AdminNote { get; set; }
        public int[] SeasonList { get; set; }
        public int SeasonCount { get; set; }
        public string SeasonsRequested { get; set; }
        public List<string> RequestedUsers { get; set; }

        public bool UserHasRequested(string username)
        {
            bool alreadyRequested = !string.IsNullOrEmpty(RequestedBy) && RequestedBy.Equals(username, StringComparison.OrdinalIgnoreCase);
            if (!alreadyRequested && RequestedUsers != null && RequestedUsers.Count > 0)
            {
                alreadyRequested = RequestedUsers.Any(x => x.Equals(username, StringComparison.OrdinalIgnoreCase)); 
            }
            return alreadyRequested;
        }
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
