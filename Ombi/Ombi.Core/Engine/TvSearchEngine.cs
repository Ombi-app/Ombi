using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using AutoMapper;
using Ombi.Api.TvMaze;
using Ombi.Api.TvMaze.Models;
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

        public TvSearchEngine(IPrincipal identity, IRequestService service, ITvMazeApi tvMaze, IMapper mapper, ISettingsService<PlexSettings> plexSettings, ISettingsService<EmbySettings> embySettings) 
            : base(identity, service)
        {
            TvMazeApi = tvMaze;
            Mapper = mapper;
            PlexSettings = plexSettings;
            EmbySettings = embySettings;
        }

        private ITvMazeApi TvMazeApi { get; }
        private IMapper Mapper { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<EmbySettings> EmbySettings { get; }


        public async Task<IEnumerable<SearchTvShowViewModel>> Search(string searchTerm)
        {
            var searchResult = await TvMazeApi.Search(searchTerm);
            
            if (searchResult != null)
            {
                return await ProcessResults(searchResult);
            }
            return null;
        }

        private async Task<IEnumerable<SearchTvShowViewModel>> ProcessResults(IEnumerable<TvMazeSearch> items)
        {
            var existingRequests = await GetRequests(RequestType.TvShow);

            var plexSettings = await PlexSettings.GetSettingsAsync();
            var embySettings = await EmbySettings.GetSettingsAsync();

            var retVal = new List<SearchTvShowViewModel>();
            foreach (var tvMazeSearch in items)
            {
                retVal.Add(ProcessResult(tvMazeSearch, existingRequests, plexSettings, embySettings));
            }
            return retVal;
        }

        private SearchTvShowViewModel ProcessResult(TvMazeSearch item, Dictionary<int, RequestModel> existingRequests, PlexSettings plexSettings, EmbySettings embySettings)
        {
            var viewT = Mapper.Map<SearchTvShowViewModel>(item);
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

            if (item.show?.externals?.thetvdb != null && !viewT.Available)
            {
                var tvdbid = (int)item.show.externals.thetvdb;
                if (existingRequests.ContainsKey(tvdbid))
                {
                    var dbt = existingRequests[tvdbid];

                    viewT.Requested = true;
                    viewT.Episodes = dbt.Episodes.ToList();
                    viewT.Approved = dbt.Approved;
                }
                //if (sonarrCached.Select(x => x.TvdbId).Contains(tvdbid) || sickRageCache.Contains(tvdbid))
                //    // compare to the sonarr/sickrage db
                //{
                //    viewT.Requested = true;
                //}
            }

            return viewT;
        }
    }
}
