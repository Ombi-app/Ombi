using System;
using System.Collections.Generic;
using Ombi.Store.Entities;

namespace Ombi.Core.Models.Requests
{
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

    public class SeasonRequestModel
    {
        public int SeasonNumber { get; set; }
        public List<int> Episodes { get; set; }
    }

}