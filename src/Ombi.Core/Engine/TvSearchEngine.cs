using AutoMapper;

using Ombi.Api.Trakt;
using Ombi.Api.TvMaze;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Core.Rules;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Store.Repository;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Ombi.Core.Models.Requests.Tv;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Engine
{
    public class TvSearchEngine : BaseMediaEngine, ITvSearchEngine
    {
        public TvSearchEngine(IPrincipal identity, IRequestServiceMain service, ITvMazeApi tvMaze, IMapper mapper, ISettingsService<PlexSettings> plexSettings,
            ISettingsService<EmbySettings> embySettings, IPlexContentRepository repo, ITraktApi trakt, IRuleEvaluator r)
            : base(identity, service, r)
        {
            TvMazeApi = tvMaze;
            Mapper = mapper;
            PlexSettings = plexSettings;
            EmbySettings = embySettings;
            PlexContentRepo = repo;
            TraktApi = trakt;
        }

        private ITvMazeApi TvMazeApi { get; }
        private IMapper Mapper { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<EmbySettings> EmbySettings { get; }
        private IPlexContentRepository PlexContentRepo { get; }
        private ITraktApi TraktApi { get; }

        public async Task<IEnumerable<SearchTvShowViewModel>> Search(string searchTerm)
        {
            var searchResult = await TvMazeApi.Search(searchTerm);

            if (searchResult != null)
            {
                return await ProcessResults(searchResult);
            }
            return null;
        }

        public async Task<SearchTvShowViewModel> GetShowInformation(int tvdbid)
        {
            var show = await TvMazeApi.ShowLookupByTheTvDbId(tvdbid);
            var episodes = await TvMazeApi.EpisodeLookup(show.id);

            var mapped = Mapper.Map<SearchTvShowViewModel>(show);

            foreach (var e in episodes)
            {
                var season = mapped.SeasonRequests.FirstOrDefault(x => x.SeasonNumber == e.season);
                if (season == null)
                {
                    var newSeason = new SeasonRequestModel
                    {
                        SeasonNumber = e.season,
                    };
                    newSeason.Episodes.Add(new EpisodesRequested
                    {
                        Url = e.url,
                        Title = e.name,
                        AirDate = DateTime.Parse(e.airstamp),
                        EpisodeNumber = e.number,
                    });
                    mapped.SeasonRequests.Add(newSeason);
                }
                else
                {
                    // Find the episode
                    var ep = season.Episodes.FirstOrDefault(x => x.EpisodeNumber == e.number);
                    ep.Url = e.url;
                    ep.Title = e.name;
                    ep.AirDate = DateTime.Parse(e.airstamp);
                    ep.EpisodeNumber = e.number;
                }
            }

            var existingRequests = await GetTvRequests();
            var plexSettings = await PlexSettings.GetSettingsAsync();
            var embySettings = await EmbySettings.GetSettingsAsync();
            return await ProcessResult(mapped, existingRequests, plexSettings, embySettings);
        }

        public async Task<IEnumerable<SearchTvShowViewModel>> Popular()
        {
            var result = await TraktApi.GetPopularShows();
            return await ProcessResults(result);
        }

        public async Task<IEnumerable<SearchTvShowViewModel>> Anticipated()
        {
            var result = await TraktApi.GetAnticipatedShows();
            return await ProcessResults(result);
        }

        public async Task<IEnumerable<SearchTvShowViewModel>> MostWatches()
        {
            var result = await TraktApi.GetMostWatchesShows();
            return await ProcessResults(result);
        }

        public async Task<IEnumerable<SearchTvShowViewModel>> Trending()
        {
            var result = await TraktApi.GetTrendingShows();
            return await ProcessResults(result);
        }

        private async Task<IEnumerable<SearchTvShowViewModel>> ProcessResults<T>(IEnumerable<T> items)
        {
            var existingRequests = await GetTvRequests();

            var plexSettings = await PlexSettings.GetSettingsAsync();
            var embySettings = await EmbySettings.GetSettingsAsync();

            var retVal = new List<SearchTvShowViewModel>();
            foreach (var tvMazeSearch in items)
            {
                var viewT = Mapper.Map<SearchTvShowViewModel>(tvMazeSearch);
                retVal.Add(await ProcessResult(viewT, existingRequests, plexSettings, embySettings));
            }
            return retVal;
        }

        private async Task<SearchTvShowViewModel> ProcessResult(SearchTvShowViewModel item, Dictionary<int, TvRequests> existingRequests, PlexSettings plexSettings, EmbySettings embySettings)
        {
            if (embySettings.Enable)
            {
                //var embyShow = EmbyChecker.GetTvShow(embyCached.ToArray(), t.show.name, t.show.premiered?.Substring(0, 4), providerId);
                //if (embyShow != null)
                //{
                //    viewT.Available = true;
                //}
            }
            if (plexSettings.Enable)
            {
                var content = await PlexContentRepo.Get(item.Id.ToString());

                if (content != null)
                {
                    item.Available = true;
                    item.PlexUrl = content.Url;
                }
            }

            if (item.Id > 0)
            {
                // TODO need to check if the episodes are available
                var tvdbid = item.Id;
                if (existingRequests.ContainsKey(tvdbid))
                {
                    var existingRequest = existingRequests[tvdbid];

                    item.Requested = true;
                    item.Approved = existingRequest.ChildRequests.Any(x => x.Approved);

                    // Let's modify the seasonsrequested to reflect what we have requested...
                    foreach (var season in item.SeasonRequests)
                    {
                        foreach (var existingRequestChildRequest in existingRequest.ChildRequests)
                        {
                            // Find the existing request season
                            var existingSeason =
                                existingRequestChildRequest.SeasonRequests.FirstOrDefault(x => x.SeasonNumber == season.SeasonNumber);
                            if(existingSeason == null) continue;

                            foreach (var ep in existingSeason.Episodes)
                            {
                                // Find the episode from what we are searching
                                var episodeSearching = season.Episodes.FirstOrDefault(x => x.EpisodeNumber == ep.EpisodeNumber);
                                episodeSearching.Requested = true;
                                episodeSearching.Available = ep.Available;
                                episodeSearching.Approved = ep.Season.ChildRequest.Approved;
                            }
                        }
                    }
                }
                //if (sonarrCached.Select(x => x.TvdbId).Contains(tvdbid) || sickRageCache.Contains(tvdbid))
                //    // compare to the sonarr/sickrage db
                //{
                //    item.Requested = true;
                //}
            }

            return item;
        }
    }
}