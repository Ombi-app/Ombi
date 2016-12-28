using System;
using System.Collections.Generic;
using System.Linq;
using Dapper.Contrib.Extensions;
using Newtonsoft.Json;

namespace Ombi.Store
{
    [Table("Requested")]
    public class RequestedModel : Entity
    {
        public RequestedModel()
        {
            RequestedUsers = new List<string>();
            Episodes = new List<EpisodesModel>();
        }

        // ReSharper disable once IdentifierTypo
        public int ProviderId { get; set; }
        public string ImdbId { get; set; }
        public string TvDbId { get; set; }
        public string Overview { get; set; }
        public string Title { get; set; }
        public string PosterPath { get; set; }
        public DateTime ReleaseDate { get; set; }
        public RequestType Type { get; set; }
        public string Status { get; set; }
        public bool Approved { get; set; }

        [Obsolete("Use RequestedUsers")] //TODO remove this obsolete property
        public string RequestedBy { get; set; }

        public DateTime RequestedDate { get; set; }
        public bool Available { get; set; }
        public IssueState Issues { get; set; }
        public string OtherMessage { get; set; }
        public string AdminNote { get; set; }
        public int[] SeasonList { get; set; }
        public int SeasonCount { get; set; }
        public string SeasonsRequested { get; set; }
        public string MusicBrainzId { get; set; }
        public List<string> RequestedUsers { get; set; }
        public string ArtistName { get; set; }
        public string ArtistId { get; set; }
        public int IssueId { get; set; }
        public List<EpisodesModel> Episodes { get; set; }
        public bool Denied { get; set; }
        public string DeniedReason { get; set; }

        [JsonIgnore]
        public List<string> AllUsers
        {
            get
            {
                var u = new List<string>();
                if (!string.IsNullOrEmpty(RequestedBy))
                {
                    u.Add(RequestedBy);
                }

                if (RequestedUsers.Any())
                {
                    u.AddRange(RequestedUsers.Where(requestedUser => requestedUser != RequestedBy));
                }
                return u;
            }
        }

        [JsonIgnore]
        public bool CanApprove => !Approved && !Available;

        public bool UserHasRequested(string username)
        {
            return AllUsers.Any(x => x.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
    }

    public enum RequestType
    {
        Movie,
        TvShow,
        Album
    }

    public static class RequestTypeDisplay
    {
        public static string GetString(this RequestType type)
        {
            switch (type)
            {
                case RequestType.Movie:
                    return "Movie";
                case RequestType.TvShow:
                    return "TV Show";
                case RequestType.Album:
                    return "Album";
                default:
                    return string.Empty;
            }
        } 
    }

    public enum IssueState
    {
        None = 99,
        WrongAudio = 0,
        NoSubtitles = 1,
        WrongContent = 2,
        PlaybackIssues = 3,
        Other = 4, // Provide a message
    }

    public class EpisodesModel : IEquatable<EpisodesModel>
    {
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public bool Equals(EpisodesModel other)
        {
            // Check whether the compared object is null.
            if (ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (ReferenceEquals(this, other)) return true;

            //Check whether the properties are equal.
            return SeasonNumber.Equals(other.SeasonNumber) && EpisodeNumber.Equals(other.EpisodeNumber);
        }

        public override int GetHashCode()
        {
            var hashSeason = SeasonNumber.GetHashCode();
            var hashEp = EpisodeNumber.GetHashCode();
            
            //Calculate the hash code.
            return hashSeason + hashEp;
        }
    }
}
