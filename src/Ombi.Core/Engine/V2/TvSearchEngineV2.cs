using System;
using AutoMapper;
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
using Ombi.Api.Trakt;
using Ombi.Api.TvMaze;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Core.Models.Search.V2;
using Ombi.Core.Settings;
using Ombi.Store.Repository;
using TraktSharp.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using System.Diagnostics;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.UI;
using Ombi.Core.Helpers;

namespace Ombi.Core.Engine.V2
{
    public class TvSearchEngineV2 : BaseMediaEngine, ITVSearchEngineV2
    {
        private readonly ITvMazeApi _tvMaze;
        private readonly IMapper _mapper;
        private readonly ITraktApi _traktApi;
        private readonly IMovieDbApi _movieApi;
        private readonly ISettingsService<CustomizationSettings> _customization;
        private readonly ITvRequestEngine _requestEngine;

        public TvSearchEngineV2(ICurrentUser identity, IRequestServiceMain service, ITvMazeApi tvMaze, IMapper mapper,
            ITraktApi trakt, IRuleEvaluator r, OmbiUserManager um, ICacheService memCache, ISettingsService<OmbiSettings> s,
            IRepository<RequestSubscription> sub, IMovieDbApi movieApi, ISettingsService<CustomizationSettings> customization, ITvRequestEngine requestEngine)
            : base(identity, service, r, um, memCache, s, sub)
        {
            _tvMaze = tvMaze;
            _mapper = mapper;
            _traktApi = trakt;
            _movieApi = movieApi;
            _customization = customization;
            _requestEngine = requestEngine;
        }


        public async Task<SearchFullInfoTvShowViewModel> GetShowByRequest(int requestId, CancellationToken token)
        {
            var request = await RequestService.TvRequestService.Get().FirstOrDefaultAsync(x => x.Id == requestId);
            return await GetShowInformation(request.ExternalProviderId.ToString(), token);
        }

        public async Task<SearchFullInfoTvShowViewModel> GetShowInformation(string tvdbid, CancellationToken token)
        {
            var langCode = await DefaultLanguageCode(null);
            var show = await Cache.GetOrAddAsync(nameof(GetShowInformation) + langCode + tvdbid,
              async () => await _movieApi.GetTVInfo(tvdbid, langCode), DateTimeOffset.Now.AddHours(12));
            if (show == null || show.name == null)
            {
                // We don't have enough information
                return null;
            }

            if (!show.Images?.Posters?.Any() ?? false && !string.Equals(langCode, "en", StringComparison.OrdinalIgnoreCase))
            {
                // There's no regional assets for this, so 
                // lookup the en-us version to get them
                var enShow = await Cache.GetOrAddAsync(nameof(GetShowInformation) + "en" + tvdbid,
                    async () => await _movieApi.GetTVInfo(tvdbid, "en"), DateTimeOffset.Now.AddHours(12));

                // For some of the more obsecure cases
                if (!show.overview.HasValue())
                {
                    show.overview = enShow.overview;
                }

                show.Images = enShow.Images;
            }

            var mapped = _mapper.Map<SearchFullInfoTvShowViewModel>(show);


            foreach (var tvSeason in show.seasons.Where(x => x.season_number != 0)) // skip the first season
            {
                var seasonEpisodes = (await _movieApi.GetSeasonEpisodes(show.id, tvSeason.season_number, token, langCode));

                MapSeasons(mapped.SeasonRequests, tvSeason, seasonEpisodes);
            }

            return await ProcessResult(mapped);
        }

        public async Task<IEnumerable<SearchTvShowViewModel>> Popular(int currentlyLoaded, int amountToLoad, string langCustomCode = null)
        {
            var langCode = await DefaultLanguageCode(langCustomCode);

            var pages = PaginationHelper.GetNextPages(currentlyLoaded, amountToLoad, ResultLimit);
            var results = new List<MovieDbSearchResult>();
            foreach (var pagesToLoad in pages)
            {
                var apiResult = await Cache.GetOrAddAsync(nameof(Popular) + langCode + pagesToLoad.Page,
                 async () => await _movieApi.PopularTv(langCode, pagesToLoad.Page), DateTimeOffset.Now.AddHours(12));
                results.AddRange(apiResult.Skip(pagesToLoad.Skip).Take(pagesToLoad.Take));
            }

            var processed = ProcessResults(results);
            return await processed;
        }

        public async Task<IEnumerable<SearchTvShowViewModel>> Anticipated(int currentlyLoaded, int amountToLoad)
        {
            var langCode = await DefaultLanguageCode(null);

            var pages = PaginationHelper.GetNextPages(currentlyLoaded, amountToLoad, ResultLimit);
            var results = new List<MovieDbSearchResult>();
            foreach (var pagesToLoad in pages)
            {
                var apiResult = await Cache.GetOrAddAsync(nameof(Anticipated) + langCode + pagesToLoad.Page,
                    async () => await _movieApi.UpcomingTv(langCode, pagesToLoad.Page), DateTimeOffset.Now.AddHours(12));
                results.AddRange(apiResult.Skip(pagesToLoad.Skip).Take(pagesToLoad.Take));
            }
            var processed = ProcessResults(results);
            return await processed;
        }

        public async Task<IEnumerable<SearchTvShowViewModel>> Trending(int currentlyLoaded, int amountToLoad)
        {
            var langCode = await DefaultLanguageCode(null);

            var pages = PaginationHelper.GetNextPages(currentlyLoaded, amountToLoad, ResultLimit);
            var results = new List<MovieDbSearchResult>();
            foreach (var pagesToLoad in pages)
            {
                var apiResult = await Cache.GetOrAddAsync(nameof(Trending) + langCode + pagesToLoad.Page,
                    async () => await _movieApi.TopRatedTv(langCode, pagesToLoad.Page), DateTimeOffset.Now.AddHours(12));
                results.AddRange(apiResult.Skip(pagesToLoad.Skip).Take(pagesToLoad.Take));
            }

            var processed = ProcessResults(results);
            return await processed;
        }

        public async Task<ActorCredits> GetTvByActor(int actorId, string langCode)
        {
            langCode = await DefaultLanguageCode(langCode);
            var result = await Cache.GetOrAddAsync(nameof(GetTvByActor) + actorId + langCode,
                () =>  _movieApi.GetActorTvCredits(actorId, langCode), DateTimeOffset.Now.AddHours(12));
            return result;
        }

        public async Task<IEnumerable<StreamingData>> GetStreamInformation(int movieDbId, CancellationToken cancellationToken)
        {
            var providers = await _movieApi.GetTvWatchProviders(movieDbId, cancellationToken);
            var results = await GetUserWatchProvider(providers);

            var data = new List<StreamingData>();

            foreach (var result in results)
            {
                data.Add(new StreamingData
                {
                    Logo = result.logo_path,
                    Order = result.display_priority,
                    StreamingProvider = result.provider_name
                });
            }

            return data;
        }


        public async Task<IEnumerable<SearchFullInfoTvShowViewModel>> RecentlyRequestedShows(int currentlyLoaded, int toLoad, CancellationToken cancellationToken)
        {
            var langCode = await DefaultLanguageCode(null);

            var results = new List<SearchFullInfoTvShowViewModel>();

            var requestResult = await Cache.GetOrAddAsync(nameof(RecentlyRequestedShows) + "Requests" + toLoad + langCode,
                async () =>
                {
                    return await _requestEngine.GetRequests(toLoad, currentlyLoaded, new Models.UI.OrderFilterModel
                    {
                        OrderType = OrderType.RequestedDateDesc
                    });
                }, DateTimeOffset.Now.AddMinutes(15));

            var movieDBResults = await Cache.GetOrAddAsync(nameof(RecentlyRequestedShows) + toLoad + langCode,
                async () =>
                {
                    var responses = new List<TvInfo>();
                    foreach (var movie in requestResult.Collection)
                    {
                        responses.Add(await _movieApi.GetTVInfo(movie.ExternalProviderId.ToString()));
                    }
                    return responses;
                }, DateTimeOffset.Now.AddHours(12));

            var mapped = _mapper.Map<List<SearchFullInfoTvShowViewModel>>(movieDBResults);
  
            foreach(var map in mapped)
            {
                var processed = await ProcessResult(map);
                results.Add(processed);
            }
            return results;
        }

        private async Task<IEnumerable<SearchTvShowViewModel>> ProcessResults(List<MovieDbSearchResult> items)
        {
            var retVal = new List<SearchTvShowViewModel>(); 
            var settings = await _customization.GetSettingsAsync();

            foreach (var tvMazeSearch in items)
            {
                if (DemoCheck(tvMazeSearch.Title))
                {
                    continue;
                }
                if (settings.HideAvailableFromDiscover)
                {
                    // To hide, we need to know if it's fully available, the only way to do this is to lookup it's episodes to check if we have every episode
                    var show = await Cache.GetOrAddAsync(nameof(GetShowInformation) + tvMazeSearch.Id.ToString(),
                        async () => await _movieApi.GetTVInfo(tvMazeSearch.Id.ToString()), DateTime.Now.AddHours(12));
                    foreach (var tvSeason in show.seasons.Where(x => x.season_number != 0)) // skip the first season
                    {
                        var seasonEpisodes = await Cache.GetOrAddAsync("SeasonEpisodes" + show.id + tvSeason.season_number, async () =>
                        {
                            return await _movieApi.GetSeasonEpisodes(show.id, tvSeason.season_number, CancellationToken.None);
                        }, DateTimeOffset.Now.AddHours(12));

                        MapSeasons(tvMazeSearch.SeasonRequests, tvSeason, seasonEpisodes);
                    }
                }

                var result = await ProcessResult(tvMazeSearch);
                if (result == null || settings.HideAvailableFromDiscover && result.FullyAvailable)
                {
                    continue;
                }
                retVal.Add(result);
            }

            return retVal;
        }

        private static void MapSeasons(List<SeasonRequests> seasonRequests, Season tvSeason, SeasonDetails seasonEpisodes)
        {
            foreach (var episode in seasonEpisodes.episodes)
            {
                var season = seasonRequests.FirstOrDefault(x => x.SeasonNumber == episode.season_number);
                if (season == null)
                {
                    var newSeason = new SeasonRequests
                    {
                        SeasonNumber = episode.season_number,
                        Overview = tvSeason.overview,
                        Episodes = new List<EpisodeRequests>()
                    };
                    newSeason.Episodes.Add(new EpisodeRequests
                    {
                        //Url = episode...ToHttpsUrl(),
                        Title = episode.name,
                        AirDate = episode.air_date.HasValue() ? DateTime.Parse(episode.air_date) : DateTime.MinValue,
                        EpisodeNumber = episode.episode_number,

                    });
                    seasonRequests.Add(newSeason);
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

        private async Task<SearchTvShowViewModel> ProcessResult<T>(T tvMazeSearch)
        {
            var item = _mapper.Map<SearchTvShowViewModel>(tvMazeSearch);

            await RunSearchRules(item);
            return item;
        }

        private async Task<SearchFullInfoTvShowViewModel> ProcessResult(SearchFullInfoTvShowViewModel item)
        {
            item.TheMovieDbId = item.Id.ToString();

            var oldModel = _mapper.Map<SearchTvShowViewModel>(item);
            await RunSearchRules(oldModel);

            item.Available = oldModel.Available;
            item.FullyAvailable = oldModel.FullyAvailable;
            item.PartlyAvailable = oldModel.PartlyAvailable;
            item.Requested = oldModel.Requested;
            item.Available = oldModel.Available;
            item.Denied = oldModel.Denied;
            item.DeniedReason = oldModel.DeniedReason;
            item.Approved = oldModel.Approved;
            item.SeasonRequests = oldModel.SeasonRequests;
            item.RequestId = oldModel.RequestId;
            item.PlexUrl = oldModel.PlexUrl;
            item.EmbyUrl = oldModel.EmbyUrl;
            item.JellyfinUrl = oldModel.JellyfinUrl;

            if (!string.IsNullOrEmpty(item.Images?.Medium))
            {
                item.Images.Medium = item.Images.Medium.ToHttpsUrl();
            }

           
            return item;
            //return await GetExtraInfo(showInfoTask, item);
        }

        private async Task<SearchFullInfoTvShowViewModel> GetExtraInfo(Task<TraktShow> showInfoTask, SearchFullInfoTvShowViewModel model)
        {
            if (showInfoTask != null)
            {
                var result = await showInfoTask;
                if (result == null)
                {
                    return model;
                }

                model.Trailer = result.Trailer?.AbsoluteUri.ToHttpsUrl() ?? string.Empty;
                model.Certification = result.Certification;
                model.Homepage = result.Homepage?.AbsoluteUri.ToHttpsUrl() ?? string.Empty;
            }
            return model;
        }
    }
}