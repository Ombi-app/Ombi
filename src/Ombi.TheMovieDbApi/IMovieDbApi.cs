﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.TheMovieDb.Models;
using Ombi.TheMovieDbApi.Models;

namespace Ombi.Api.TheMovieDb
{
    public interface IMovieDbApi
    {
        Task<MovieResponseDto> GetMovieInformation(int movieId);
        Task<MovieResponseDto> GetMovieInformationWithExtraInfo(int movieId, string langCode = "en");
        Task<List<MovieSearchResult>> NowPlaying(string languageCode, int? page = null);
        Task<List<MovieSearchResult>> PopularMovies(string languageCode, int? page = null);
        Task<List<MovieSearchResult>> SearchMovie(string searchTerm, int? year, string languageCode);
        Task<List<TvSearchResult>> SearchTv(string searchTerm);
        Task<List<MovieSearchResult>> TopRated(string languageCode, int? page = null);
        Task<List<MovieSearchResult>> Upcoming(string languageCode, int? page = null);
        Task<List<MovieSearchResult>> SimilarMovies(int movieId, string langCode);
        Task<FindResult> Find(string externalId, ExternalSource source);
        Task<TvExternals> GetTvExternals(int theMovieDbId);
        Task<TvInfo> GetTVInfo(string themoviedbid);
        Task<TheMovieDbContainer<ActorResult>> SearchByActor(string searchTerm, string langCode);
        Task<ActorCredits> GetActorMovieCredits(int actorId, string langCode);
        Task<TheMovieDbContainer<MultiSearch>> MultiSearch(string searchTerm, string languageCode);
        Task<FullMovieInfo> GetFullMovieInfo(int movieId, string langCode);
        Task<TheMovieDbContainer<DiscoverMovies>> DiscoverMovies(string langCode, int keywordId);
        Task<Collections> GetCollection(string langCode, int collectionId);
    }
}