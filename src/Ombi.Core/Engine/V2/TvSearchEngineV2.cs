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

namespace Ombi.Core.Engine.V2
{
    public class TvSearchEngineV2 : BaseMediaEngine, ITVSearchEngineV2
    {
        private readonly ITvMazeApi _tvMaze;
        private readonly IMapper _mapper;
        private readonly ITraktApi _traktApi;
        private readonly IMovieDbApi _movieApi;

        public TvSearchEngineV2(IPrincipal identity, IRequestServiceMain service, ITvMazeApi tvMaze, IMapper mapper,
            ITraktApi trakt, IRuleEvaluator r, OmbiUserManager um, ICacheService memCache, ISettingsService<OmbiSettings> s,
            IRepository<RequestSubscription> sub, IMovieDbApi movieApi)
            : base(identity, service, r, um, memCache, s, sub)
        {
            _tvMaze = tvMaze;
            _mapper = mapper;
            _traktApi = trakt;
            _movieApi = movieApi;
        }


        public async Task<SearchFullInfoTvShowViewModel> GetShowByRequest(int requestId)
        {
            var request = await RequestService.TvRequestService.Get().FirstOrDefaultAsync(x => x.Id == requestId);
            return await GetShowInformation(request.TvDbId);
        }

        public async Task<SearchFullInfoTvShowViewModel> GetShowInformation(int tvdbid)
        {
            var tvdbshow = await Cache.GetOrAdd(nameof(GetShowInformation) + tvdbid,
                async () => await _tvMaze.ShowLookupByTheTvDbId(tvdbid), DateTime.Now.AddHours(12));
            if (tvdbshow == null)
            {
                return null;
            }
            var show = await Cache.GetOrAdd("GetTvFullInformation" + tvdbshow.id,
                async () => await _tvMaze.GetTvFullInformation(tvdbshow.id), DateTime.Now.AddHours(12));
            if (show == null)
            {
                // We don't have enough information
                return null;
            }

            // Setup the task so we can get the data later on if we have a IMDBID
            Task<TraktShow> traktInfoTask = null;
            if (show.externals?.imdb.HasValue() ?? false)
            {
                traktInfoTask = Cache.GetOrAdd("GetExtendedTvInfoTrakt" + show.externals?.imdb,
                    () => _traktApi.GetTvExtendedInfo(show.externals?.imdb), DateTime.Now.AddHours(12));
            }

            var mapped = _mapper.Map<SearchFullInfoTvShowViewModel>(show);

            foreach (var e in show._embedded?.episodes ?? new Api.TvMaze.Models.V2.Episode[0])
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
                        Url = e.url.ToHttpsUrl(),
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
                        Url = e.url.ToHttpsUrl(),
                        Title = e.name,
                        AirDate = e.airstamp,
                        EpisodeNumber = e.number,
                    });
                }
            }
            return await ProcessResult(mapped, traktInfoTask);
        }

        public async Task<IEnumerable<StreamingData>> GetStreamInformation(int tvDbId, int tvMazeId, CancellationToken cancellationToken)
        {
            var tvdbshow = await Cache.GetOrAdd(nameof(GetShowInformation) + tvMazeId,
                async () => await _tvMaze.ShowLookupByTheTvDbId(tvMazeId), DateTime.Now.AddHours(12));
            if (tvdbshow == null)
            {
                return null;
            }

            /// this is a best effort guess since TV maze do not provide the TheMovieDbId
            var movieDbResults = await _movieApi.SearchTv(tvdbshow.name, tvdbshow.premiered.Substring(0, 4));
            var potential = movieDbResults.FirstOrDefault();
            tvDbId = potential.Id;
            // end guess

            var providers = await _movieApi.GetTvWatchProviders(tvDbId, cancellationToken);
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
            return _mapper.Map<SearchTvShowViewModel>(tvMazeSearch);
        }

        private async Task<SearchFullInfoTvShowViewModel> ProcessResult(SearchFullInfoTvShowViewModel item, Task<TraktShow> showInfoTask)
        {
            item.TheTvDbId = item.Id.ToString();

            var oldModel = _mapper.Map<SearchTvShowViewModel>(item);
            await RunSearchRules(oldModel);

            item.Available = oldModel.Available;
            item.FullyAvailable = oldModel.FullyAvailable;
            item.PartlyAvailable = oldModel.PartlyAvailable;
            item.Requested = oldModel.Requested;
            item.Available = oldModel.Available;
            item.Approved = oldModel.Approved;
            item.SeasonRequests = oldModel.SeasonRequests;
            item.RequestId = oldModel.RequestId;

            if (!string.IsNullOrEmpty(item.Images?.Medium))
            {
                item.Images.Medium = item.Images.Medium.ToHttpsUrl();
            }

            if (item.Cast?.Any() ?? false)
            {
                foreach (var cast in item.Cast)
                {
                    if (!string.IsNullOrEmpty(cast.Character?.Image?.Medium))
                    {
                        cast.Character.Image.Medium = cast.Character?.Image?.Medium.ToHttpsUrl();
                    }
                }
            }

            return await GetExtraInfo(showInfoTask, item);
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