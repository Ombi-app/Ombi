using System;
using System.Collections.Generic;
using AutoMapper;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Models.Search;
using Ombi.Core.Models.Search.V2;
using Ombi.TheMovieDbApi.Models;
using Keywords = Ombi.Core.Models.Search.V2.Keywords;
using KeywordsValue = Ombi.Api.TheMovieDb.Models.KeywordsValue;

namespace Ombi.Mapping.Profiles
{
    public class MovieProfile : Profile
    {
        public MovieProfile()
        {
            CreateMap<SearchResult, MovieSearchResult>()
                .ForMember(dest => dest.Adult, opts => opts.MapFrom(src => src.adult))
                .ForMember(dest => dest.BackdropPath, opts => opts.MapFrom(src => src.backdrop_path))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.id))
                .ForMember(dest => dest.OriginalLanguage, opts => opts.MapFrom(src => src.original_language))
                .ForMember(dest => dest.OriginalTitle, opts => opts.MapFrom(src => src.original_title))
                .ForMember(dest => dest.Overview, opts => opts.MapFrom(src => src.overview))
                .ForMember(dest => dest.Popularity, opts => opts.MapFrom(src => src.popularity))
                .ForMember(dest => dest.PosterPath, opts => opts.MapFrom(src => src.poster_path))
                .ForMember(dest => dest.ReleaseDate, opts => opts.MapFrom(src => src.release_date))
                .ForMember(dest => dest.Title, opts => opts.MapFrom(src => src.title))
                .ForMember(dest => dest.Video, opts => opts.MapFrom(src => src.video))
                .ForMember(dest => dest.VoteAverage, opts => opts.MapFrom(src => src.vote_average))
                .ForMember(dest => dest.VoteCount, opts => opts.MapFrom(src => src.vote_count));

            CreateMap<SearchResult, TvSearchResult>()
                .ForMember(dest => dest.BackdropPath, opts => opts.MapFrom(src => src.backdrop_path))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.id))
                .ForMember(dest => dest.OriginalLanguage, opts => opts.MapFrom(src => src.original_language))
                .ForMember(dest => dest.OriginalName, opts => opts.MapFrom(src => src.original_name))
                .ForMember(dest => dest.Overview, opts => opts.MapFrom(src => src.overview))
                .ForMember(dest => dest.Popularity, opts => opts.MapFrom(src => src.popularity))
                .ForMember(dest => dest.PosterPath, opts => opts.MapFrom(src => src.poster_path))
                .ForMember(dest => dest.ReleaseDate, opts => opts.MapFrom(src => src.first_air_date))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.name))
                .ForMember(dest => dest.VoteAverage, opts => opts.MapFrom(src => src.vote_average))
                .ForMember(dest => dest.VoteCount, opts => opts.MapFrom(src => src.vote_count));

            CreateMap<MovieResponse, MovieResponseDto>()
                .ForMember(dest => dest.Adult, opts => opts.MapFrom(src => src.adult))
                .ForMember(dest => dest.BackdropPath, opts => opts.MapFrom(src => src.backdrop_path))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.id))
                .ForMember(dest => dest.OriginalLanguage, opts => opts.MapFrom(src => src.original_language))
                .ForMember(dest => dest.OriginalTitle, opts => opts.MapFrom(src => src.original_title))
                .ForMember(dest => dest.Overview, opts => opts.MapFrom(src => src.overview))
                .ForMember(dest => dest.Popularity, opts => opts.MapFrom(src => src.popularity))
                .ForMember(dest => dest.PosterPath, opts => opts.MapFrom(src => src.poster_path))
                .ForMember(dest => dest.ReleaseDate, opts => opts.MapFrom(src => src.release_date))
                .ForMember(dest => dest.Title, opts => opts.MapFrom(src => src.title))
                .ForMember(dest => dest.Video, opts => opts.MapFrom(src => src.video))
                .ForMember(dest => dest.VoteAverage, opts => opts.MapFrom(src => src.vote_average))
                .ForMember(dest => dest.ImdbId, opts => opts.MapFrom(src => src.imdb_id))
                .ForMember(dest => dest.Homepage, opts => opts.MapFrom(src => src.homepage))
                .ForMember(dest => dest.Runtime, opts => opts.MapFrom(src => src.runtime))
                .ForMember(dest => dest.Status, opts => opts.MapFrom(src => src.status))
                .ForMember(dest => dest.Tagline, opts => opts.MapFrom(src => src.tagline))
                .ForMember(dest => dest.VoteCount, opts => opts.MapFrom(src => src.vote_count))
                .ForMember(dest => dest.ReleaseDates, opts => opts.MapFrom(src => src.release_dates));

            CreateMap<ReleaseDates, ReleaseDatesDto>()
                .ForMember(x => x.Results, o => o.MapFrom(src => src.results));

            CreateMap<ReleaseResults, ReleaseResultsDto>()
                .ForMember(x => x.ReleaseDate, o => o.MapFrom(s => s.release_dates))
                .ForMember(x => x.IsoCode, o => o.MapFrom(s => s.iso_3166_1));
            CreateMap<ReleaseDate, ReleaseDateDto>()
                .ForMember(x => x.ReleaseDate, o => o.MapFrom(s => s.release_date))
                .ForMember(x => x.Type, o => o.MapFrom(s => s.Type));

            CreateMap<TheMovieDbApi.Models.Genre, GenreDto>();

            CreateMap<MovieSearchResult, SearchMovieViewModel>().ReverseMap();
            CreateMap<MovieResponseDto, SearchMovieViewModel>().ReverseMap();

            CreateMap<FullMovieInfo, SearchMovieViewModel>().ReverseMap();
            CreateMap<ProductionCompanies, Production_Companies>().ReverseMap();
            CreateMap<CreditsViewModel, Credits>().ReverseMap();
            CreateMap<MovieFullInfoViewModel, FullMovieInfo>().ReverseMap();
            CreateMap<Ombi.Api.TheMovieDb.Models.Genre, Ombi.Core.Models.Search.V2.GenreViewModel>().ReverseMap();
            CreateMap<Ombi.Api.TheMovieDb.Models.Production_Companies, Ombi.Core.Models.Search.V2.ProductionCompaniesViewModel>().ReverseMap();
            CreateMap<Ombi.Api.TheMovieDb.Models.Videos, Ombi.Core.Models.Search.V2.Videos>().ReverseMap();
            CreateMap<Ombi.Api.TheMovieDb.Models.Result, Ombi.Core.Models.Search.V2.VideoResultsDetails>().ReverseMap();
            CreateMap<Ombi.Api.TheMovieDb.Models.FullMovieCast, Ombi.Core.Models.Search.V2.FullMovieCastViewModel>().ReverseMap();
            CreateMap<Ombi.Api.TheMovieDb.Models.FullMovieCrew, Ombi.Core.Models.Search.V2.FullMovieCrewViewModel>().ReverseMap();
            CreateMap<Ombi.Api.TheMovieDb.Models.ExternalIds, Ombi.Core.Models.Search.V2.ExternalIds>().ReverseMap();
            CreateMap<BelongsToCollection, Ombi.Core.Models.Search.V2.CollectionsViewModel>().ReverseMap();
            CreateMap<Api.TheMovieDb.Models.Keywords, Ombi.Core.Models.Search.V2.Keywords>().ReverseMap();
            CreateMap<KeywordsValue, Ombi.Core.Models.Search.V2.KeywordsValue>().ReverseMap();

            CreateMap<Collections, Ombi.Core.Models.Search.V2.MovieCollectionsViewModel>()
                .ForMember(x => x.Name, o => o.MapFrom(s => s.name))
                .ForMember(x => x.Overview, o => o.MapFrom(s => s.overview))
                .ForMember(x => x.Collection, o => o.MapFrom(s => s.parts));

            CreateMap<Part, MovieCollection>()
                .ForMember(x => x.Id, o => o.MapFrom(s => s.id))
                .ForMember(x => x.Overview, o => o.MapFrom(s => s.overview))
                .ForMember(x => x.PosterPath, o => o.MapFrom(s => s.poster_path))
                .ForMember(x => x.ReleaseDate, o => o.MapFrom(s => DateTime.Parse(s.release_date)))
                .ForMember(x => x.Title, o => o.MapFrom(s => s.title));

            CreateMap<SearchMovieViewModel, MovieCollection>().ReverseMap();
        }
    }
}