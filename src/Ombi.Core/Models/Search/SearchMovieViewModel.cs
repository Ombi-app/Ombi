﻿using System;
using System.Collections.Generic;
using Ombi.Store.Entities;

namespace Ombi.Core.Models.Search
{
    public class SearchMovieViewModel : SearchViewModel
    {
        public bool Adult { get; set; }
        public string BackdropPath { get; set; }
        public List<int> GenreIds { get; set; }
        public string OriginalLanguage { get; set; }
        public string OriginalTitle { get; set; }
        public string Overview { get; set; }
        public double Popularity { get; set; }
        public string PosterPath { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string Title { get; set; }
        public bool Video { get; set; }
        public double VoteAverage { get; set; }
        public int VoteCount { get; set; }
        public bool AlreadyInCp { get; set; }
        public string Trailer { get; set; }
        public string Homepage { get; set; }
        public int RootPathOverride { get; set; }
        public int QualityOverride { get; set; }
        public override RequestType Type => RequestType.Movie;
    }
}