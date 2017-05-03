using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using AutoMapper;
using Ombi.Api.Trakt;
using Ombi.Api.TvMaze;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Core.Requests.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Store.Entities;

namespace Ombi.Core.Engine
{
    public class TvSearchEngine : BaseMediaEngine, ITvSearchEngine
    {

        public TvSearchEngine(IPrincipal identity, IRequestServiceMain service, ITvMazeApi tvMaze, IMapper mapper, ISettingsService<PlexSettings> plexSettings,
            ISettingsService<EmbySettings> embySettings, ITraktApi trakt) 
            : base(identity, service)
        {
            TvMazeApi = tvMaze;
            Mapper = mapper;
            PlexSettings = plexSettings;
            EmbySettings = embySettings;
            TraktApi = trakt;
        }

        private ITvMazeApi TvMazeApi { get; }
        private IMapper Mapper { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<EmbySettings> EmbySettings { get; }
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
                retVal.Add(ProcessResult(viewT, existingRequests, plexSettings, embySettings));
            }
            return retVal;
        }

        private SearchTvShowViewModel ProcessResult(SearchTvShowViewModel item, Dictionary<int, TvRequestModel> existingRequests, PlexSettings plexSettings, EmbySettings embySettings)
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
                //var plexShow = PlexChecker.GetTvShow(plexTvShows.ToArray(), t.show.name, t.show.premiered?.Substring(0, 4),
                //    providerId);
                //if (plexShow != null)
                //{
                //    viewT.Available = true;
                //    viewT.PlexUrl = plexShow.Url;
                //}
            }

            if (item.Id > 0 && item.Available)
            {
                var tvdbid = item.Id;
                if (existingRequests.ContainsKey(tvdbid))
                {
                    var dbt = existingRequests[tvdbid];

                    item.Requested = true;
                    item.EpisodesRequested = dbt.Episodes.ToList();
                    item.Approved = dbt.Approved;
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
