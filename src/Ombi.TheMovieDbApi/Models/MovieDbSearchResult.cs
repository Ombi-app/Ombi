using Ombi.Store.Repository.Requests;
using System.Collections.Generic;

namespace Ombi.Api.TheMovieDb.Models
{
    public class MovieDbSearchResult
    {
        public string PosterPath { get; set; }
        public bool Adult { get; set; }
        public string Overview { get; set; }
        public string ReleaseDate { get; set; }
        public int[] GenreIds { get; set; }
        public int Id { get; set; }
        public string OriginalTitle { get; set; }
        public string OriginalLanguage { get; set; }
        public string Title { get; set; }
        public string BackdropPath { get; set; }
        public float Popularity { get; set; }
        public int VoteCount { get; set; }
        public bool Video { get; set; }
        public float VoteAverage { get; set; }

        /// <summary>
        /// Mapped Property and not set from the API
        /// </summary>
        public List<SeasonRequests> SeasonRequests { get; set; } = new List<SeasonRequests>();
    }
}