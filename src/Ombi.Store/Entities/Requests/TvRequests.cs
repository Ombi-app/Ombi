using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities.Requests
{
    public class TvRequests : Entity
    {
        /// <summary>
        /// No longer used since moved away from TvMaze
        /// </summary>
        public int TvDbId { get; set; }

        // TheMovieDbId
        public int ExternalProviderId { get; set; }
        public string ImdbId { get; set; }
        public int? QualityOverride { get; set; }
        public int? RootFolder { get; set; }
        public string Overview { get; set; }
        public string Title { get; set; }
        public string PosterPath { get; set; }
        public string Background { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Status { get; set; }

        public int TotalSeasons { get; set; }

        public List<ChildRequests> ChildRequests { get; set; }
    }
}