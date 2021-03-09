﻿using AutoMapper;

using Ombi.Api.Trakt;
using Ombi.Api.TvMaze;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Core.Settings;
using Ombi.Store.Repository;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Repository.Requests;
using Ombi.Core.Authentication;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using System.Threading;

namespace Ombi.Core.Engine
{
    public class TvSearchEngine : BaseMediaEngine, ITvSearchEngine
    {
        private readonly ISettingsService<CustomizationSettings> _customizationSettings;
        private readonly IImageService _imageService;
        private readonly IMovieDbApi _theMovieDbApi;

        public TvSearchEngine(IPrincipal identity, IRequestServiceMain service, ITvMazeApi tvMaze, IMapper mapper,
            ITraktApi trakt, IRuleEvaluator r, OmbiUserManager um, ISettingsService<CustomizationSettings> customizationSettings,
            ICacheService memCache, ISettingsService<OmbiSettings> s, IRepository<RequestSubscription> sub, IImageService imageService,
            IMovieDbApi theMovieDbApi)
            : base(identity, service, r, um, memCache, s, sub)
        {
            _imageService = imageService;
            _theMovieDbApi = theMovieDbApi;
            TvMazeApi = tvMaze;
            Mapper = mapper;
            TraktApi = trakt;
            _customizationSettings = customizationSettings;
        }

        protected ITvMazeApi TvMazeApi { get; }
        protected IMapper Mapper { get; }
        private ITraktApi TraktApi { get; }

        public async Task<IEnumerable<SearchTvShowViewModel>> Search(string searchTerm)
        {
            var searchResult = await _theMovieDbApi.SearchTv(searchTerm);

            if (searchResult != null)
            {
                var retVal = new List<SearchTvShowViewModel>();
                foreach (var result in searchResult)
                {
                    //if (tvMazeSearch.show.externals == null || !(tvMazeSearch.show.externals?.thetvdb.HasValue ?? false))
                    //{
                    //    continue;
                    //}
                    var mappedResult = await ProcessResult(result, false);
                    if (mappedResult == null)
                    {
                        continue;
                    }
                    retVal.Add(mappedResult);
                }
                return retVal;
            }
            return null;
        }

        public async Task<SearchTvShowViewModel> GetShowInformation(string theMovieDbId, CancellationToken token)
        {
            var show = await Cache.GetOrAdd(nameof(GetShowInformation) + theMovieDbId,
                async () => await _theMovieDbApi.GetTVInfo(theMovieDbId), DateTime.Now.AddHours(12));
            if (show == null)
            {
                // We don't have enough information
                return null;
            }

            //var episodes = await Cache.GetOrAdd("TvMazeEpisodeLookup" + show.id,
            //    async () => await TvMazeApi.EpisodeLookup(show.id), DateTime.Now.AddHours(12));
            //if (episodes == null || !episodes.Any())
            //{
            //    // We don't have enough information
            //    return null;
            //}

            var mapped = Mapper.Map<SearchTvShowViewModel>(show);

            foreach(var tvSeason in show.seasons)
            {
                var seasonEpisodes = (await _theMovieDbApi.GetSeasonEpisodes(show.id, tvSeason.season_number, token));

                foreach (var episode in seasonEpisodes.episodes)
                {
                    var season = mapped.SeasonRequests.FirstOrDefault(x => x.SeasonNumber == episode.season_number);
                    if (season == null)
                    {
                        var newSeason = new SeasonRequests
                        {
                            SeasonNumber = episode.season_number,
                            Episodes = new List<EpisodeRequests>()
                        };
                        newSeason.Episodes.Add(new EpisodeRequests
                        {
                            //Url = episode...ToHttpsUrl(),
                            Title = episode.name,
                            AirDate = episode.air_date.HasValue() ? DateTime.Parse(episode.air_date) : DateTime.MinValue,
                            EpisodeNumber = episode.episode_number,

                        });
                        mapped.SeasonRequests.Add(newSeason);
                    }
                    else
                    {
                        // We already have the season, so just add the episode
                        season.Episodes.Add(new EpisodeRequests
                        {
                            //Url = e.url.ToHttpsUrl(),
                            Title = episode.name,
                            AirDate = episode.air_date.HasValue() ? DateTime.Parse(episode.air_date) : DateTime.MinValue,
                            EpisodeNumber = episode.episode_number,
                        });
                    }
                }
            }

            return await ProcessResult(mapped, false);
        }

        public async Task<IEnumerable<SearchTvShowViewModel>> Popular()
        {
            var result = await Cache.GetOrAdd(CacheKeys.PopularTv, async () => await TraktApi.GetPopularShows(null, ResultLimit), DateTime.Now.AddHours(12));
            var processed = ProcessResults(result);
            return await processed;
        }

        public async Task<IEnumerable<SearchTvShowViewModel>> Popular(int currentlyLoaded, int amountToLoad, bool includeImages = false)
        {
            var langCode = await DefaultLanguageCode(null);

            var pages = PaginationHelper.GetNextPages(currentlyLoaded, amountToLoad, ResultLimit);
            var results = new List<MovieDbSearchResult>();
            foreach (var pagesToLoad in pages)
            {
                var apiResult = await Cache.GetOrAdd(nameof(Popular) + langCode + pagesToLoad.Page,
                    async () => await _theMovieDbApi.PopularTv(langCode, pagesToLoad.Page), DateTime.Now.AddHours(12));
                results.AddRange(apiResult.Skip(pagesToLoad.Skip).Take(pagesToLoad.Take));
            }

            var processed = ProcessResults(results, includeImages);
            return await processed;
        }

        public async Task<IEnumerable<SearchTvShowViewModel>> Anticipated()
        {

            var result = await Cache.GetOrAdd(CacheKeys.AnticipatedTv, async () => await TraktApi.GetAnticipatedShows(null, ResultLimit), DateTime.Now.AddHours(12));
            var processed = ProcessResults(result);
            return await processed;
        }

        public async Task<IEnumerable<SearchTvShowViewModel>> Anticipated(int currentlyLoaded, int amountToLoad)
        {
            var langCode = await DefaultLanguageCode(null);

            var pages = PaginationHelper.GetNextPages(currentlyLoaded, amountToLoad, ResultLimit);
            var results = new List<MovieDbSearchResult>();
            foreach (var pagesToLoad in pages)
            {
                var apiResult = await Cache.GetOrAdd(nameof(Anticipated) + langCode + pagesToLoad.Page,
                    async () => await _theMovieDbApi.UpcomingTv(langCode, pagesToLoad.Page), DateTime.Now.AddHours(12));
                results.AddRange(apiResult.Skip(pagesToLoad.Skip).Take(pagesToLoad.Take));
            }
            var processed = ProcessResults(results);
            return await processed;
        }

        public async Task<IEnumerable<SearchTvShowViewModel>> Trending()
        {
            var result = await Cache.GetOrAdd(CacheKeys.TrendingTv, async () => await TraktApi.GetTrendingShows(null, ResultLimit), DateTime.Now.AddHours(12));
            var processed = ProcessResults(result);
            return await processed;
        }


        public async Task<IEnumerable<SearchTvShowViewModel>> Trending(int currentlyLoaded, int amountToLoad)
        {
            var langCode = await DefaultLanguageCode(null);

            var pages = PaginationHelper.GetNextPages(currentlyLoaded, amountToLoad, ResultLimit);
            var results = new List<MovieDbSearchResult>();
            foreach (var pagesToLoad in pages)
            {
                var apiResult = await Cache.GetOrAdd(nameof(Trending) + langCode + pagesToLoad.Page,
                    async () => await _theMovieDbApi.TopRatedTv(langCode, pagesToLoad.Page), DateTime.Now.AddHours(12));
                results.AddRange(apiResult.Skip(pagesToLoad.Skip).Take(pagesToLoad.Take));
            }
            var processed = ProcessResults(results);
            return await processed;
        }

        protected async Task<IEnumerable<SearchTvShowViewModel>> ProcessResults<T>(IEnumerable<T> items, bool includeImages = false)
        {
            var retVal = new List<SearchTvShowViewModel>();
            var settings = await _customizationSettings.GetSettingsAsync();
            foreach (var tvMazeSearch in items)
            {
                var result = await ProcessResult(tvMazeSearch, includeImages);
                if (result == null || settings.HideAvailableFromDiscover && result.Available)
                {
                    continue;
                }
                retVal.Add(result);
            }
            return retVal;
        }

        protected async Task<SearchTvShowViewModel> ProcessResult<T>(T tvMazeSearch, bool includeImages)
        {
            var mapped = Mapper.Map<SearchTvShowViewModel>(tvMazeSearch);
            return await ProcessResult(mapped, includeImages);
        }

        private async Task<SearchTvShowViewModel> ProcessResult(SearchTvShowViewModel item, bool includeImages)
        {
            if (item.Id == 0)
            {
                return null;
            }
            item.TheMovieDbId = item.Id.ToString();
            //item.TheTvDbId = item.Id.ToString();
            //if (includeImages)
            //{
            //    if (item.TheTvDbId.HasValue())
            //    {
            //        item.BackdropPath = await _imageService.GetTvBackground(item.TheTvDbId);
            //    }
            //}

            await RunSearchRules(item);

            return item;
        }
    }
}