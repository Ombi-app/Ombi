using AutoMapper;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Models.Search;
using Ombi.TheMovieDbApi.Models;

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
        }
    }
}