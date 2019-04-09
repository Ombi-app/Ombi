using AutoMapper;

using Ombi.Api.Trakt;
using Ombi.Api.TvMaze;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Store.Repository;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Repository.Requests;
using Microsoft.Extensions.Caching.Memory;
using Ombi.Core.Authentication;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;

namespace Ombi.Core.Engine
{
    public class TvSearchEngine : BaseMediaEngine, ITvSearchEngine
    {
        public TvSearchEngine(IPrincipal identity, IRequestServiceMain service, ITvMazeApi tvMaze, IMapper mapper, ISettingsService<PlexSettings> plexSettings,
            ISettingsService<EmbySettings> embySettings, IPlexContentRepository repo, IEmbyContentRepository embyRepo, ITraktApi trakt, IRuleEvaluator r, OmbiUserManager um,
            ICacheService memCache, ISettingsService<OmbiSettings> s, IRepository<RequestSubscription> sub)
            : base(identity, service, r, um, memCache, s, sub)
        {
            TvMazeApi = tvMaze;
            Mapper = mapper;
            PlexSettings = plexSettings;
            EmbySettings = embySettings;
            PlexContentRepo = repo;
            TraktApi = trakt;
            EmbyContentRepo = embyRepo;
        }

        protected ITvMazeApi TvMazeApi { get; }
        protected IMapper Mapper { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<EmbySettings> EmbySettings { get; }
        private IPlexContentRepository PlexContentRepo { get; }
        private IEmbyContentRepository EmbyContentRepo { get; }
        private ITraktApi TraktApi { get; }

        public async Task<IEnumerable<SearchTvShowViewModel>> Search(string searchTerm)
        {
            var searchResult = await TvMazeApi.Search(searchTerm);

            if (searchResult != null)
            {
                var retVal = new List<SearchTvShowViewModel>();
                foreach (var tvMazeSearch in searchResult)
                {
                    if (tvMazeSearch.show.externals == null || !(tvMazeSearch.show.externals?.thetvdb.HasValue ?? false))
                    {
                        continue;
                    }
                    retVal.Add(ProcessResult(tvMazeSearch));
                }
                return retVal;
            }
            return null;
        }

        public async Task<SearchTvShowViewModel> GetShowInformation(int tvdbid)
        {
            var show = await TvMazeApi.ShowLookupByTheTvDbId(tvdbid);
            if (show == null)
            { 
                // We don't have enough information
                return null;
            }
            var episodes = await TvMazeApi.EpisodeLookup(show.id);
            if (episodes == null || !episodes.Any())
            {
                // We don't have enough information
                return null;
            }

            var mapped = Mapper.Map<SearchTvShowViewModel>(show);

            foreach (var e in episodes)
            {
                var season = mapped.SeasonRequests.FirstOrDefault(x => x.SeasonNumber == e.season);
                if (season == null)
                {
                    var newSeason = new SeasonRequests
                    {
                        SeasonNumber = e.season,
                        Episodes = new List<EpisodeRequests>()
                    };
                    newSeason.Episodes.Add(new EpisodeRequests
                    {
                        Url = e.url,
                        Title = e.name,
                        AirDate = e.airstamp.HasValue() ? DateTime.Parse(e.airstamp) : DateTime.MinValue,
                        EpisodeNumber = e.number,

                    });
                    mapped.SeasonRequests.Add(newSeason);
                }
                else
                {
                    // We already have the season, so just add the episode
                    season.Episodes.Add(new EpisodeRequests
                    {
                        Url = e.url,
                        Title = e.name,
                        AirDate = e.airstamp.HasValue() ? DateTime.Parse(e.airstamp) : DateTime.MinValue,
                        EpisodeNumber = e.number,
                    });
                }
            }
            return await ProcessResult(mapped);
        }

        public async Task<IEnumerable<SearchTvShowViewModel>> Popular()
        {
            var result = await Cache.GetOrAdd(CacheKeys.PopularTv, async () => await TraktApi.GetPopularShows(), DateTime.Now.AddHours(12));
            var processed = ProcessResults(result);
            return processed;
        }

        public async Task<IEnumerable<SearchTvShowViewModel>> Anticipated()
        {

            var result = await Cache.GetOrAdd(CacheKeys.AnticipatedTv, async () => await TraktApi.GetAnticipatedShows(), DateTime.Now.AddHours(12));
            var processed = ProcessResults(result);
            return processed;
        }

        public async Task<IEnumerable<SearchTvShowViewModel>> MostWatches()
        {
            var result = await Cache.GetOrAdd(CacheKeys.MostWatchesTv, async () => await TraktApi.GetMostWatchesShows(), DateTime.Now.AddHours(12));
            var processed = ProcessResults(result);
            return processed;
        }

        public async Task<IEnumerable<SearchTvShowViewModel>> Trending()
        {
            var result = await Cache.GetOrAdd(CacheKeys.TrendingTv, async () => await TraktApi.GetTrendingShows(), DateTime.Now.AddHours(12));
            var processed = ProcessResults(result);
            return processed;
        }

        protected IEnumerable<SearchTvShowViewModel> ProcessResults<T>(IEnumerable<T> items)
        {
            var retVal = new List<SearchTvShowViewModel>();
            foreach (var tvMazeSearch in items)
            {
                retVal.Add(ProcessResult(tvMazeSearch));
            }
            return retVal;
        }

        protected SearchTvShowViewModel ProcessResult<T>(T tvMazeSearch)
        {
            return Mapper.Map<SearchTvShowViewModel>(tvMazeSearch);
        }

        private async Task<SearchTvShowViewModel> ProcessResult(SearchTvShowViewModel item)
        {
            item.TheTvDbId = item.Id.ToString();
            
            await RunSearchRules(item);

            return item;
        }
    }
}