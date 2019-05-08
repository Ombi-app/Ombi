using System;
using System.Collections.Generic;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Store.Entities;

namespace Ombi.Core.Models.Search.V2
{
    public class MovieFullInfoViewModel : SearchViewModel
    {
        public bool Adult { get; set; }
        public CollectionsViewModel BelongsToCollection { get; set; }
        public string BackdropPath { get; set; }
        public string OriginalLanguage { get; set; }
        public int Budget { get; set; }
        public GenreViewModel[] Genres { get; set; }
        public string OriginalTitle { get; set; }
        public string Overview { get; set; }
        public List<ProductionCompaniesViewModel> ProductionCompanies { get; set; }
        public double Popularity { get; set; }
        public float Revenue { get; set; }
        public long Runtime { get; set; }
        public string PosterPath { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string Title { get; set; }
        public bool Video { get; set; }
        public string Tagline { get; set; }
        public double VoteAverage { get; set; }
        public int VoteCount { get; set; }
        public bool AlreadyInCp { get; set; }
        public string Trailer { get; set; }
        public string Homepage { get; set; }
        public int RootPathOverride { get; set; }
        public string Status { get; set; }
        public Videos Videos { get; set; }
        public CreditsViewModel Credits { get; set; }
        public int QualityOverride { get; set; }
        public override RequestType Type => RequestType.Movie;
        public ReleaseDatesDto ReleaseDates { get; set; }
        public DateTime? DigitalReleaseDate { get; set; }
        public Similar Similar { get; set; }
        public Recommendations Recommendations { get; set; }
        public ExternalIds ExternalIds { get; set; }
        public Keywords Keywords { get; set; }
    }
    public class Keywords
    {
        public List<KeywordsValue> KeywordsValue { get; set; }
    }

    public class KeywordsValue
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CollectionsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PosterPath { get; set; }
        public string BackdropPath { get; set; }
    }
    public class ExternalIds
    {
        public string ImdbId { get; set; }
        public string FacebookId { get; set; }
        public string InstagramId { get; set; }
        public string TwitterId { get; set; }
    }
    public class GenreViewModel
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class ProductionCompaniesViewModel
    {
        public int id { get; set; }
        public string logo_path { get; set; }
        public string name { get; set; }
        public string origin_country { get; set; }
    }

    public class Videos
    {
        public VideoResultsDetails[] results { get; set; }
    }

    public class VideoResultsDetails
    {
        public string id { get; set; }
        public string iso_639_1 { get; set; }
        public string iso_3166_1 { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public string site { get; set; }
        public int size { get; set; }
        public string type { get; set; }
    }
    public class CreditsViewModel
    {
        public FullMovieCastViewModel[] cast { get; set; }
        public FullMovieCrewViewModel[] crew { get; set; }
    }
    public class FullMovieCastViewModel
    {
        public int cast_id { get; set; }
        public string character { get; set; }
        public string credit_id { get; set; }
        public int gender { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int order { get; set; }
        public string profile_path { get; set; }
    }

    public class FullMovieCrewViewModel
    {
        public string credit_id { get; set; }
        public string department { get; set; }
        public int gender { get; set; }
        public int id { get; set; }
        public string job { get; set; }
        public string name { get; set; }
        public string profile_path { get; set; }
    }
}