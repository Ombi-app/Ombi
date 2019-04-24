﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;
using Ombi.Core.Models.Search.V2;

namespace Ombi.Core.Engine.Interfaces
{
    public interface IMovieEngineV2
    {
        Task<MovieFullInfoViewModel> GetFullMovieInformation(int theMovieDbId, string langCode = null);
        Task<IEnumerable<SearchMovieViewModel>> SimilarMovies(int theMovieDbId, string langCode);
        Task<IEnumerable<SearchMovieViewModel>> PopularMovies();
        Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies();
        Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies();
        Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies();
        Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies(int currentPosition, int amountToLoad);
        Task<MovieCollectionsViewModel> GetCollection(int collectionId, string langCode = null);
        Task<int> GetTvDbId(int theMovieDbId);
        Task<IEnumerable<SearchMovieViewModel>> PopularMovies(int currentlyLoaded, int toLoad);
        Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies(int currentlyLoaded, int toLoad);
        Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies(int currentlyLoaded, int toLoad);
        int ResultLimit { get; set; }
    }
}