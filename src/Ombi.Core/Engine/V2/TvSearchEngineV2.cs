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
using Ombi.Core.Settings.Models.External;
using Ombi.Store.Repository;
using TraktSharp.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ombi.Core.Engine.V2
{
    public class TvSearchEngineV2 : BaseMediaEngine, ITVSearchEngineV2
    {
        public TvSearchEngineV2(IPrincipal identity, IRequestServiceMain service, ITvMazeApi tvMaze, IMapper mapper, ISettingsService<PlexSettings> plexSettings,
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

        private ITvMazeApi TvMazeApi { get; }
        private IMapper Mapper { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<EmbySettings> EmbySettings { get; }
        private IPlexContentRepository PlexContentRepo { get; }
        private IEmbyContentRepository EmbyContentRepo { get; }
        private ITraktApi TraktApi { get; }

        public async Task<SearchFullInfoTvShowViewModel> GetShowByRequest(int requestId)
        {
            var request = await RequestService.TvRequestService.GetChild().Include(x => x.ParentRequest).FirstOrDefaultAsync(x => x.Id == requestId);
            return await GetShowInformation(request.ParentRequest.TvDbId);
        }

        public async Task<SearchFullInfoTvShowViewModel> GetShowInformation(int tvdbid)
        {
            var tvdbshow = await Cache.GetOrAdd(nameof(GetShowInformation) + tvdbid,
                async () => await TvMazeApi.ShowLookupByTheTvDbId(tvdbid), DateTime.Now.AddHours(12));
            if (tvdbshow == null)
            {
                return null;
            }
            var show = await Cache.GetOrAdd("GetTvFullInformation" + tvdbshow.id,
                async () => await TvMazeApi.GetTvFullInformation(tvdbshow.id), DateTime.Now.AddHours(12));
            if (show == null)
            {
                // We don't have enough information
                return null;
            }

            // Setup the task so we can get the data later on if we have a IMDBID
            Task<TraktShow> traktInfoTask = new Task<TraktShow>(() => null);
            if (show.externals?.imdb.HasValue() ?? false)
            {
                traktInfoTask = Cache.GetOrAdd("GetExtendedTvInfoTrakt" + show.externals?.imdb,
                    () => TraktApi.GetTvExtendedInfo(show.externals?.imdb), DateTime.Now.AddHours(12));
            }

            var mapped = Mapper.Map<SearchFullInfoTvShowViewModel>(show);

            foreach (var e in show._embedded.episodes)
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
                        AirDate = e.airstamp,
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
                        AirDate = e.airstamp,
                        EpisodeNumber = e.number,
                    });
                }
            }
            return await ProcessResult(mapped, traktInfoTask);
        }

        private IEnumerable<SearchTvShowViewModel> ProcessResults<T>(IEnumerable<T> items)
        {
            var retVal = new List<SearchTvShowViewModel>();
            foreach (var tvMazeSearch in items)
            {
                retVal.Add(ProcessResult(tvMazeSearch));
            }
            return retVal;
        }

        private SearchTvShowViewModel ProcessResult<T>(T tvMazeSearch)
        {
            return Mapper.Map<SearchTvShowViewModel>(tvMazeSearch);
        }

        private async Task<SearchFullInfoTvShowViewModel> ProcessResult(SearchFullInfoTvShowViewModel item, Task<TraktShow> showInfoTask)
        {
            item.TheTvDbId = item.Id.ToString();

            var oldModel = Mapper.Map<SearchTvShowViewModel>(item);
            await RunSearchRules(oldModel);

            item.Available = oldModel.Available;
            item.FullyAvailable = oldModel.FullyAvailable;
            item.PartlyAvailable = oldModel.PartlyAvailable;
            item.Requested = oldModel.Requested;
            item.Available = oldModel.Available;
            item.Approved = oldModel.Approved;
            item.SeasonRequests = oldModel.SeasonRequests;
            item.RequestId = oldModel.RequestId;

            return await GetExtraInfo(showInfoTask, item);
        }

        private async Task<SearchFullInfoTvShowViewModel> GetExtraInfo(Task<TraktShow> showInfoTask, SearchFullInfoTvShowViewModel model)
        {
            var result = await showInfoTask;
            if (result == null)
            {
                return model;
            }

            model.Trailer = result.Trailer?.AbsoluteUri ?? string.Empty;
            model.Certification = result.Certification;
            model.Homepage = result.Homepage?.AbsoluteUri ?? string.Empty;

            return model;
        }
    }
}