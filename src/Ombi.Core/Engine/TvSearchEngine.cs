using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using AutoMapper;
using Ombi.Api.Trakt;
using Ombi.Api.TvMaze;
using Ombi.Api.TvMaze.Models;
using Ombi.Core.Engine.Interfaces;
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
            ISettingsService<EmbySettings> embySettings) 
            : base(identity, service)
        {
            TvMazeApi = tvMaze;
            Mapper = mapper;
            PlexSettings = plexSettings;
            EmbySettings = embySettings;
            //TraktApi = trakt;
        }

        private ITvMazeApi TvMazeApi { get; }
        private IMapper Mapper { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<EmbySettings> EmbySettings { get; }
        //private ITraktApi TraktApi { get; }


        public async Task<IEnumerable<SearchTvShowViewModel>> Search(string searchTerm)
        {
            var searchResult = await TvMazeApi.Search(searchTerm);
            
            if (searchResult != null)
            {
                return await ProcessResults(searchResult);
            }
            return null;
        }

        public async Task<SearchTvShowViewModel> GetShowInformation(int tvmazeId)
        {
            var show = await TvMazeApi.ShowLookup(tvmazeId);
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
                    season.Episodes.Add(new EpisodesRequested
                    {
                        Url = e.url,
                        Title = e.name,
                        AirDate = DateTime.Parse(e.airstamp),
                        EpisodeNumber = e.number,

                    });
                }
            }

            var existingRequests = await GetTvRequests();
            var plexSettings = await PlexSettings.GetSettingsAsync();
            var embySettings = await EmbySettings.GetSettingsAsync();
            return ProcessResult(mapped, existingRequests, plexSettings, embySettings);
        }

        //public async Task<IEnumerable<SearchTvShowViewModel>> Popular()
        //{
        //    var result = await TraktApi.GetPopularShows();
        //    return await ProcessResults(result);
        //}

        //public async Task<IEnumerable<SearchTvShowViewModel>> Anticipated()
        //{
        //    var result = await TraktApi.GetAnticipatedShows();
        //    return await ProcessResults(result);
        //}
        //public async Task<IEnumerable<SearchTvShowViewModel>> MostWatches()
        //{
        //    var result = await TraktApi.GetMostWatchesShows();
        //    return await ProcessResults(result);
        //}
        //public async Task<IEnumerable<SearchTvShowViewModel>> Trending()
        //{
        //    var result = await TraktApi.GetTrendingShows();
        //    return await ProcessResults(result);
        //}

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
                    var existingRequest = existingRequests[tvdbid];

                    item.Requested = true;
                    item.Approved = existingRequest.Approved;

                    // Let's modify the seasonsrequested to reflect what we have requested...
                    foreach (var season in item.SeasonRequests)
                    {
                        // Find the existing request season
                        var existingSeason =
                            existingRequest.SeasonRequests.FirstOrDefault(x => x.SeasonNumber == season.SeasonNumber);

                        foreach (var ep in existingSeason.Episodes)
                        {
                           // Find the episode from what we are searching
                            var episodeSearching = season.Episodes.FirstOrDefault(x => x.EpisodeNumber == ep.EpisodeNumber);
                            episodeSearching.Requested = ep.Requested;
                            episodeSearching.Available = ep.Available;
                            episodeSearching.Approved = ep.Approved;
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
