using Ombi.Api.Interfaces;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Services.Interfaces;
using Ombi.Services.Notification;
using Ombi.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using Ombi.Api.Models.Sonarr;
using Ombi.Api.Models.Tv;
using TraktApiSharp.Objects.Get.Shows;
using EpisodesModel = Ombi.Store.EpisodesModel;
using System.Threading.Tasks;
using Ombi.UI.Models;
using Nancy;
using Ombi.UI.Helpers;
using Ombi.Core.Models;
using Ombi.Store.Models;
using Action = Ombi.Helpers.Analytics.Action;
using Ombi.Helpers.Analytics;
using Ombi.Helpers;
using Nancy.Extensions;
using Ombi.Api;
using System.Globalization;
using Ombi.Helpers.Permissions;
using Newtonsoft.Json;
using Ombi.Services.Jobs;
using Ombi.Core.Queue;
using Ombi.Store.Repository;
using Ombi.Store.Models.Plex;
using Ombi.Store.Models.Emby;

namespace Ombi.UI.Modules.Search
{
    public class TvSearchModule : BaseSearchModule
    {
        protected ISonarrCacher SonarrCacher { get; }
        protected ISickRageCacher SickRageCacher { get; }
        protected ISettingsService<SonarrSettings> SonarrService { get; }
        protected ISettingsService<SickRageSettings> SickRageService { get; }
        protected ISonarrApi SonarrApi { get; }
        protected ISickRageApi SickrageApi { get; }
        protected ITraktApi TraktApi { get; }
        protected TvMazeApi TvApi { get; }

        public TvSearchModule(ITraktApi traktApi, ISonarrApi sonarrApi, ISettingsService<SonarrSettings> sonarrSettings,
            ISettingsService<SickRageSettings> sickRageService, ISickRageApi srApi,
            ISonarrCacher sonarrCacher, ISickRageCacher sickRageCacher,
            /* parameters needed for base class constructor */
            IPlexApi plexApi, ISettingsService<PlexRequestSettings> prSettings,
            ISettingsService<PlexSettings> plexService, ISettingsService<AuthenticationSettings> auth,
            ISecurityExtensions security, IAvailabilityChecker plexChecker, INotificationService notify,
            ISettingsService<CustomizationSettings> cus, IRequestService request, IAnalytics a,
            IRepository<UsersToNotify> u, ISettingsService<EmailNotificationSettings> email,
            IIssueService issue, IRepository<RequestLimit> rl, ITransientFaultQueue tfQueue,
            IRepository<PlexContent> content, IEmbyAvailabilityChecker embyChecker,
            IRepository<EmbyContent> embyContent, ISettingsService<EmbySettings> embySettings) 
            : base(plexApi, prSettings,plexService, auth, security, plexChecker, notify, cus, request, a, u,
                  email, issue, rl, tfQueue,content, embyChecker, embyContent, embySettings)
        {
            TraktApi = traktApi;
            TvApi = new TvMazeApi();
            SonarrApi = sonarrApi;
            SonarrCacher = sonarrCacher;
            SonarrService = sonarrSettings;
            SickrageApi = srApi;
            SickRageService = sickRageService;
            SickRageCacher = sickRageCacher;

            Get["tv/{searchTerm}", true] = async (x, ct) => await SearchTvShow((string)x.searchTerm);
            Get["tv/popular", true] = async (x, ct) => await ProcessShows(ShowSearchType.Popular);
            Get["tv/trending", true] = async (x, ct) => await ProcessShows(ShowSearchType.Trending);
            Get["tv/mostwatched", true] = async (x, ct) => await ProcessShows(ShowSearchType.MostWatched);
            Get["tv/anticipated", true] = async (x, ct) => await ProcessShows(ShowSearchType.Anticipated);
            Get["tv/poster/{id}"] = p => GetTvPoster((int)p.id);
            Post["request/tv", true] =
                async (x, ct) => await RequestTvShow((int)Request.Form.tvId, (string)Request.Form.seasons);
            Post["request/tvEpisodes", true] = async (x, ct) => await RequestTvShow(0, "episode");

            Get["/seasons"] = x => GetSeasons();
            Get["/episodes", true] = async (x, ct) => await GetEpisodes();

        }


        private async Task<Response> ProcessShows(ShowSearchType type)
        {
            var shows = new List<SearchTvShowViewModel>();
            var prSettings = await PrService.GetSettingsAsync();
            switch (type)
            {
                case ShowSearchType.Popular:
                    Analytics.TrackEventAsync(Category.Search, Action.TvShow, "Popular", Username, CookieHelper.GetAnalyticClientId(Cookies));
                    var popularShows = await TraktApi.GetPopularShows();

                    foreach (var popularShow in popularShows)
                    {
                        var theTvDbId = int.Parse(popularShow.Ids.Tvdb.ToString());

                        var model = new SearchTvShowViewModel
                        {
                            FirstAired = popularShow.FirstAired?.ToString("yyyy-MM-ddTHH:mm:ss"),
                            Id = theTvDbId,
                            ImdbId = popularShow.Ids.Imdb,
                            Network = popularShow.Network,
                            Overview = popularShow.Overview.RemoveHtml(),
                            Rating = popularShow.Rating.ToString(),
                            Runtime = popularShow.Runtime.ToString(),
                            SeriesName = popularShow.Title,
                            Status = popularShow.Status.DisplayName,
                            DisableTvRequestsByEpisode = prSettings.DisableTvRequestsByEpisode,
                            DisableTvRequestsBySeason = prSettings.DisableTvRequestsBySeason,
                            EnableTvRequestsForOnlySeries = (prSettings.DisableTvRequestsByEpisode && prSettings.DisableTvRequestsBySeason),
                            Trailer = popularShow.Trailer,
                            Homepage = popularShow.Homepage
                        };
                        shows.Add(model);
                    }
                    shows = await MapToTvModel(shows, prSettings);
                    break;
                case ShowSearchType.Anticipated:
                    Analytics.TrackEventAsync(Category.Search, Action.TvShow, "Anticipated", Username, CookieHelper.GetAnalyticClientId(Cookies));
                    var anticipated = await TraktApi.GetAnticipatedShows();
                    foreach (var anticipatedShow in anticipated)
                    {
                        var show = anticipatedShow.Show;
                        var theTvDbId = int.Parse(show.Ids.Tvdb.ToString());
                        var result = TvApi.ShowLookupByTheTvDbId(theTvDbId);
                        if (result == null)
                        {
                            continue;
                        }

                        var model = new SearchTvShowViewModel
                        {
                            FirstAired = show.FirstAired?.ToString("yyyy-MM-ddTHH:mm:ss"),
                            Id = theTvDbId,
                            ImdbId = show.Ids.Imdb,
                            Network = show.Network ?? string.Empty,
                            Overview = show.Overview?.RemoveHtml() ?? string.Empty,
                            Rating = show.Rating.ToString(),
                            Runtime = show.Runtime.ToString(),
                            SeriesName = show.Title,
                            Status = show.Status?.DisplayName ?? string.Empty,
                            DisableTvRequestsByEpisode = prSettings.DisableTvRequestsByEpisode,
                            DisableTvRequestsBySeason = prSettings.DisableTvRequestsBySeason,
                            EnableTvRequestsForOnlySeries = (prSettings.DisableTvRequestsByEpisode && prSettings.DisableTvRequestsBySeason),
                            Trailer = show.Trailer,
                            Homepage = show.Homepage
                        };
                        shows.Add(model);
                    }
                    shows = await MapToTvModel(shows, prSettings);
                    break;
                case ShowSearchType.MostWatched:
                    Analytics.TrackEventAsync(Category.Search, Action.TvShow, "MostWatched", Username, CookieHelper.GetAnalyticClientId(Cookies));
                    var mostWatched = await TraktApi.GetMostWatchesShows();
                    foreach (var watched in mostWatched)
                    {
                        var show = watched.Show;
                        var theTvDbId = int.Parse(show.Ids.Tvdb.ToString());
                        var result = TvApi.ShowLookupByTheTvDbId(theTvDbId);
                        if (result == null)
                        {
                            continue;
                        }

                        var model = new SearchTvShowViewModel
                        {
                            FirstAired = show.FirstAired?.ToString("yyyy-MM-ddTHH:mm:ss"),
                            Id = theTvDbId,
                            ImdbId = show.Ids.Imdb,
                            Network = show.Network,
                            Overview = show.Overview.RemoveHtml(),
                            Rating = show.Rating.ToString(),
                            Runtime = show.Runtime.ToString(),
                            SeriesName = show.Title,
                            Status = show.Status.DisplayName,
                            DisableTvRequestsByEpisode = prSettings.DisableTvRequestsByEpisode,
                            DisableTvRequestsBySeason = prSettings.DisableTvRequestsBySeason,
                            EnableTvRequestsForOnlySeries = (prSettings.DisableTvRequestsByEpisode && prSettings.DisableTvRequestsBySeason),
                            Trailer = show.Trailer,
                            Homepage = show.Homepage
                        };
                        shows.Add(model);
                    }
                    shows = await MapToTvModel(shows, prSettings);
                    break;
                case ShowSearchType.Trending:
                    Analytics.TrackEventAsync(Category.Search, Action.TvShow, "Trending", Username, CookieHelper.GetAnalyticClientId(Cookies));
                    var trending = await TraktApi.GetTrendingShows();
                    foreach (var watched in trending)
                    {
                        var show = watched.Show;
                        var theTvDbId = int.Parse(show.Ids.Tvdb.ToString());
                        var result = TvApi.ShowLookupByTheTvDbId(theTvDbId);
                        if (result == null)
                        {
                            continue;
                        }

                        var model = new SearchTvShowViewModel
                        {
                            FirstAired = show.FirstAired?.ToString("yyyy-MM-ddTHH:mm:ss"),
                            Id = theTvDbId,
                            ImdbId = show.Ids.Imdb,
                            Network = show.Network,
                            Overview = show.Overview.RemoveHtml(),
                            Rating = show.Rating.ToString(),
                            Runtime = show.Runtime.ToString(),
                            SeriesName = show.Title,
                            Status = show.Status.DisplayName,
                            DisableTvRequestsByEpisode = prSettings.DisableTvRequestsByEpisode,
                            DisableTvRequestsBySeason = prSettings.DisableTvRequestsBySeason,
                            EnableTvRequestsForOnlySeries = (prSettings.DisableTvRequestsByEpisode && prSettings.DisableTvRequestsBySeason),
                            Trailer = show.Trailer,
                            Homepage = show.Homepage
                        };
                        shows.Add(model);
                    }
                    shows = await MapToTvModel(shows, prSettings);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }


            return Response.AsJson(shows);
        }

        private async Task<List<SearchTvShowViewModel>> MapToTvModel(List<SearchTvShowViewModel> shows, PlexRequestSettings prSettings)
        {
            var plexSettings = await PlexService.GetSettingsAsync();
            var embySettings = await EmbySettings.GetSettingsAsync();

            // Get the requests
            var allResults = await RequestService.GetAllAsync();
            allResults = allResults.Where(x => x.Type == RequestType.TvShow);
            var distinctResults = allResults.DistinctBy(x => x.ProviderId);
            var dbTv = distinctResults.ToDictionary(x => x.ImdbId);

            var content = PlexContentRepository.GetAll();
            var plexTvShows = PlexChecker.GetPlexTvShows(content);
            var embyContent = EmbyContentRepository.GetAll();
            var embyCached = EmbyChecker.GetEmbyTvShows(embyContent).ToList();

            foreach (var show in shows)
            {

                var providerId = show.Id.ToString();

                if (embySettings.Enable)
                {
                    var embyShow = EmbyChecker.GetTvShow(embyCached.ToArray(), show.SeriesName, show.FirstAired?.Substring(0, 4), providerId);
                    if (embyShow != null)
                    {
                        show.Available = true;
                    }
                }
                if (plexSettings.Enable)
                {
                    var plexShow = PlexChecker.GetTvShow(plexTvShows.ToArray(), show.SeriesName, show.FirstAired?.Substring(0, 4),
                    providerId);
                    if (plexShow != null)
                    {
                        show.Available = true;
                        show.PlexUrl = plexShow.Url;
                    }
                }

                if (show.ImdbId != null && !show.Available)
                {
                    var imdbId = show.ImdbId;
                    if (dbTv.ContainsKey(imdbId))
                    {
                        var dbt = dbTv[imdbId];

                        show.Requested = true;
                        show.Episodes = dbt.Episodes.ToList();
                        show.Approved = dbt.Approved;
                    }
                }
            }
            return shows;
        }

        private async Task<Response> SearchTvShow(string searchTerm)
        {

            Analytics.TrackEventAsync(Category.Search, Action.TvShow, searchTerm, Username,
                CookieHelper.GetAnalyticClientId(Cookies));
            var plexSettings = await PlexService.GetSettingsAsync();
            var embySettings = await EmbySettings.GetSettingsAsync();
            var prSettings = await PrService.GetSettingsAsync();
            var providerId = string.Empty;

            var apiTv = new List<TvMazeSearch>();
            await Task.Factory.StartNew(() => new TvMazeApi().Search(searchTerm)).ContinueWith((t) =>
            {
                apiTv = t.Result;
            });

            var allResults = await RequestService.GetAllAsync();
            allResults = allResults.Where(x => x.Type == RequestType.TvShow);
            var distinctResults = allResults.DistinctBy(x => x.ProviderId);
            var dbTv = distinctResults.ToDictionary(x => x.ProviderId);

            if (!apiTv.Any())
            {
                return Response.AsJson("");
            }

            var sonarrCached = SonarrCacher.QueuedIds();
            var sickRageCache = SickRageCacher.QueuedIds(); // consider just merging sonarr/sickrage arrays
            var content = PlexContentRepository.GetAll();
            var plexTvShows = PlexChecker.GetPlexTvShows(content);
            var embyContent = EmbyContentRepository.GetAll();
            var embyCached = EmbyChecker.GetEmbyTvShows(embyContent);

            var viewTv = new List<SearchTvShowViewModel>();
            foreach (var t in apiTv)
            {
                if (!(t.show.externals?.thetvdb.HasValue) ?? false)
                {
                    continue;
                }
                var banner = t.show.image?.medium;
                if (!string.IsNullOrEmpty(banner))
                {
                    banner = banner.Replace("http", "https"); // Always use the Https banners
                }

                var viewT = new SearchTvShowViewModel
                {
                    Banner = banner,
                    FirstAired = t.show.premiered,
                    Id = t.show.externals?.thetvdb ?? 0,
                    ImdbId = t.show.externals?.imdb,
                    Network = t.show.network?.name,
                    NetworkId = t.show.network?.id.ToString(),
                    Overview = t.show.summary.RemoveHtml(),
                    Rating = t.score.ToString(CultureInfo.CurrentUICulture),
                    Runtime = t.show.runtime.ToString(),
                    SeriesId = t.show.id,
                    SeriesName = t.show.name,
                    Status = t.show.status,
                    DisableTvRequestsByEpisode = prSettings.DisableTvRequestsByEpisode,
                    DisableTvRequestsBySeason = prSettings.DisableTvRequestsBySeason,
                    EnableTvRequestsForOnlySeries = (prSettings.DisableTvRequestsByEpisode && prSettings.DisableTvRequestsBySeason)
                };

                providerId = viewT.Id.ToString();

                if (embySettings.Enable)
                {
                    var embyShow = EmbyChecker.GetTvShow(embyCached.ToArray(), t.show.name, t.show.premiered?.Substring(0, 4), providerId);
                    if (embyShow != null)
                    {
                        viewT.Available = true;
                    }
                }
                if (plexSettings.Enable)
                {
                    var plexShow = PlexChecker.GetTvShow(plexTvShows.ToArray(), t.show.name, t.show.premiered?.Substring(0, 4),
                    providerId);
                    if (plexShow != null)
                    {
                        viewT.Available = true;
                        viewT.PlexUrl = plexShow.Url;
                    }
                }

                if (t.show?.externals?.thetvdb != null && !viewT.Available)
                {
                    var tvdbid = (int)t.show.externals.thetvdb;
                    if (dbTv.ContainsKey(tvdbid))
                    {
                        var dbt = dbTv[tvdbid];

                        viewT.Requested = true;
                        viewT.Episodes = dbt.Episodes.ToList();
                        viewT.Approved = dbt.Approved;
                    }
                    if (sonarrCached.Select(x => x.TvdbId).Contains(tvdbid) || sickRageCache.Contains(tvdbid))
                    // compare to the sonarr/sickrage db
                    {
                        viewT.Requested = true;
                    }
                }

                viewTv.Add(viewT);
            }

            return Response.AsJson(viewTv);
        }


        /// <summary>
        /// Requests the tv show.
        /// </summary>
        /// <param name="showId">The show identifier.</param>
        /// <param name="seasons">The seasons.</param>
        /// <returns></returns>
        private async Task<Response> RequestTvShow(int showId, string seasons)
        {
            if (Security.HasPermissions(User, Permissions.ReadOnlyUser) || !Security.HasPermissions(User, Permissions.RequestTvShow))
            {
                return
                    Response.AsJson(new JsonResponseModel()
                    {
                        Result = false,
                        Message = "Sorry, you do not have the correct permissions to request a TV Show!"
                    });
            }
            // Get the JSON from the request
            var req = (Dictionary<string, object>.ValueCollection)Request.Form.Values;
            EpisodeRequestModel episodeModel = null;
            if (req.Count == 1)
            {
                var json = req.FirstOrDefault()?.ToString();
                episodeModel = JsonConvert.DeserializeObject<EpisodeRequestModel>(json); // Convert it into the object
            }
            var episodeRequest = false;

            var settings = await PrService.GetSettingsAsync();
            if (!await CheckRequestLimit(settings, RequestType.TvShow))
            {
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = false,
                        Message = Resources.UI.Search_WeeklyRequestLimitTVShow
                    });
            }
            Analytics.TrackEventAsync(Category.Search, Action.Request, "TvShow", Username,
                CookieHelper.GetAnalyticClientId(Cookies));

            var sonarrSettings = SonarrService.GetSettingsAsync();

            // This means we are requesting an episode rather than a whole series or season
            if (episodeModel != null)
            {
                episodeRequest = true;
                showId = episodeModel.ShowId;
                var s = await sonarrSettings;
                if (!s.Enabled)
                {
                    return
                        Response.AsJson(new JsonResponseModel
                        {
                            Message =
                                "This is currently only supported with Sonarr, Please enable Sonarr for this feature",
                            Result = false
                        });
                }
            }
            var embySettings = await EmbySettings.GetSettingsAsync();
            var showInfo = TvApi.ShowLookupByTheTvDbId(showId);
            DateTime firstAir;
            DateTime.TryParse(showInfo.premiered, out firstAir);
            string fullShowName = $"{showInfo.name} ({firstAir.Year})";

            // For some reason the poster path is always http
            var posterPath = showInfo.image?.medium.Replace("http:", "https:");
            var model = new RequestedModel
            {
                Type = RequestType.TvShow,
                Overview = showInfo.summary.RemoveHtml(),
                PosterPath = posterPath,
                Title = showInfo.name,
                ReleaseDate = firstAir,
                Status = showInfo.status,
                RequestedDate = DateTime.UtcNow,
                Approved = false,
                RequestedUsers = new List<string> { Username },
                Issues = IssueState.None,
                ImdbId = showInfo.externals?.imdb ?? string.Empty,
                SeasonCount = showInfo.Season.Count,
                TvDbId = showId.ToString()
            };

            var seasonsList = new List<int>();
            switch (seasons)
            {
                case "first":
                    seasonsList.Add(1);
                    model.SeasonsRequested = "First";
                    break;
                case "latest":
                    seasonsList.Add(model.SeasonCount);
                    model.SeasonsRequested = "Latest";
                    break;
                case "all":
                    model.SeasonsRequested = "All";
                    break;
                case "episode":
                    model.Episodes = new List<EpisodesModel>();

                    foreach (var ep in episodeModel?.Episodes ?? new Models.EpisodesModel[0])
                    {
                        model.Episodes.Add(new EpisodesModel
                        {
                            EpisodeNumber = ep.EpisodeNumber,
                            SeasonNumber = ep.SeasonNumber
                        });
                    }
                    Analytics.TrackEventAsync(Category.Requests, Action.TvShow, $"Episode request for {model.Title}",
                        Username, CookieHelper.GetAnalyticClientId(Cookies));
                    break;
                default:
                    model.SeasonsRequested = seasons;
                    var split = seasons.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var seasonsCount = new int[split.Length];
                    for (var i = 0; i < split.Length; i++)
                    {
                        int tryInt;
                        int.TryParse(split[i], out tryInt);
                        seasonsCount[i] = tryInt;
                    }
                    seasonsList.AddRange(seasonsCount);
                    break;
            }

            model.SeasonList = seasonsList.ToArray();

            // check if the show/episodes have already been requested
            var existingRequest = await RequestService.CheckRequestAsync(showId);
            var difference = new List<EpisodesModel>();
            if (existingRequest != null)
            {
                if (episodeRequest)
                {
                    // Make sure we are not somehow adding dupes
                    difference = GetListDifferences(existingRequest.Episodes, episodeModel.Episodes).ToList();
                    if (difference.Any())
                    {
                        // Convert the request into the correct shape
                        var newEpisodes = episodeModel.Episodes?.Select(x => new EpisodesModel
                        {
                            SeasonNumber = x.SeasonNumber,
                            EpisodeNumber = x.EpisodeNumber
                        });

                        // Add it to the existing requests
                        existingRequest.Episodes.AddRange(newEpisodes ?? Enumerable.Empty<EpisodesModel>());

                        // It's technically a new request now, so set the status to not approved.
                        var autoApprove = ShouldAutoApprove(RequestType.TvShow);
                        if (autoApprove)
                        {
                            return await SendTv(model, sonarrSettings, existingRequest, fullShowName, settings);
                        }
                        existingRequest.Approved = false;

                        return await AddUserToRequest(existingRequest, settings, fullShowName, true);
                    }
                    else
                    {
                        // We no episodes to approve
                        return
                            Response.AsJson(new JsonResponseModel
                            {
                                Result = false,
                                Message = $"{fullShowName} {string.Format(Resources.UI.Search_AlreadyInPlex, embySettings.Enable ? "Emby" : "Plex")}"
                            });
                    }
                }
                else if (model.SeasonList.Except(existingRequest.SeasonList).Any())
                {
                    // This is a season being requested that we do not yet have
                    // Let's just continue
                }
                else
                {
                    return await AddUserToRequest(existingRequest, settings, fullShowName);
                }
            }

            try
            {

                var plexSettings = await PlexService.GetSettingsAsync();
                if (plexSettings.Enable)
                {
                    var content = PlexContentRepository.GetAll();
                    var shows = PlexChecker.GetPlexTvShows(content);

                    var providerId = string.Empty;
                    if (plexSettings.AdvancedSearch)
                    {
                        providerId = showId.ToString();
                    }
                    if (episodeRequest)
                    {
                        var cachedEpisodesTask = await PlexChecker.GetEpisodes();
                        var cachedEpisodes = cachedEpisodesTask.ToList();
                        foreach (var d in difference) // difference is from an existing request
                        {
                            if (
                                cachedEpisodes.Any(
                                    x =>
                                        x.SeasonNumber == d.SeasonNumber && x.EpisodeNumber == d.EpisodeNumber &&
                                        x.ProviderId == providerId))
                            {
                                return
                                    Response.AsJson(new JsonResponseModel
                                    {
                                        Result = false,
                                        Message =
                                            $"{fullShowName}  {d.SeasonNumber} - {d.EpisodeNumber} {string.Format(Resources.UI.Search_AlreadyInPlex, GetMediaServerName())}"
                                    });
                            }
                        }

                        var diff = await GetEpisodeRequestDifference(showId, model);
                        model.Episodes = diff.ToList();
                    }
                    else
                    {
                        if (plexSettings.EnableTvEpisodeSearching)
                        {
                            foreach (var s in showInfo.Season)
                            {
                                var result = PlexChecker.IsEpisodeAvailable(showId.ToString(), s.SeasonNumber,
                                    s.EpisodeNumber);
                                if (result)
                                {
                                    return
                                        Response.AsJson(new JsonResponseModel
                                        {
                                            Result = false,
                                            Message = $"{fullShowName} {string.Format(Resources.UI.Search_AlreadyInPlex, GetMediaServerName())}"
                                        });
                                }
                            }
                        }
                        else if (PlexChecker.IsTvShowAvailable(shows.ToArray(), showInfo.name,
                            showInfo.premiered?.Substring(0, 4),
                            providerId, model.SeasonList))
                        {
                            return
                                Response.AsJson(new JsonResponseModel
                                {
                                    Result = false,
                                    Message = $"{fullShowName} {string.Format(Resources.UI.Search_AlreadyInPlex, GetMediaServerName())}"
                                });
                        }
                    }
                }
                if (embySettings.Enable)
                {
                    var embyContent = EmbyContentRepository.GetAll();
                    var embyMovies = EmbyChecker.GetEmbyTvShows(embyContent);
                    var providerId = showId.ToString();
                    if (episodeRequest)
                    {
                        var cachedEpisodesTask = await EmbyChecker.GetEpisodes();
                        var cachedEpisodes = cachedEpisodesTask.ToList();
                        foreach (var d in difference) // difference is from an existing request
                        {
                            if (
                                cachedEpisodes.Any(
                                    x =>
                                        x.SeasonNumber == d.SeasonNumber && x.EpisodeNumber == d.EpisodeNumber &&
                                        x.ProviderId == providerId))
                            {
                                return
                                    Response.AsJson(new JsonResponseModel
                                    {
                                        Result = false,
                                        Message =
                                            $"{fullShowName}  {d.SeasonNumber} - {d.EpisodeNumber} {string.Format(Resources.UI.Search_AlreadyInPlex, GetMediaServerName())}"
                                    });
                            }
                        }

                        var diff = await GetEpisodeRequestDifference(showId, model);
                        model.Episodes = diff.ToList();
                    }
                    else
                    {
                        if (embySettings.EnableEpisodeSearching)
                        {
                            foreach (var s in showInfo.Season)
                            {
                                var result = EmbyChecker.IsEpisodeAvailable(showId.ToString(), s.SeasonNumber,
                                    s.EpisodeNumber);
                                if (result)
                                {
                                    return
                                        Response.AsJson(new JsonResponseModel
                                        {
                                            Result = false,
                                            Message = $"{fullShowName} is already in Emby!"
                                        });
                                }
                            }
                        }
                        else if (EmbyChecker.IsTvShowAvailable(embyMovies.ToArray(), showInfo.name,
                           showInfo.premiered?.Substring(0, 4),
                           providerId, model.SeasonList))
                        {
                            return
                                Response.AsJson(new JsonResponseModel
                                {
                                    Result = false,
                                    Message = $"{fullShowName} is already in Emby!"
                                });
                        }
                    }
                }
            }
            catch (Exception)
            {
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = false,
                        Message = string.Format(Resources.UI.Search_CouldNotCheckPlex, fullShowName, GetMediaServerName())
                    });
            }

            if (showInfo.externals?.thetvdb == null)
            {
                await FaultQueue.QueueItemAsync(model, showInfo.id.ToString(), RequestType.TvShow, FaultType.MissingInformation, "We do not have a TheTVDBId from TVMaze");
                await NotificationService.Publish(new NotificationModel
                {
                    DateTime = DateTime.Now,
                    User = Username,
                    RequestType = RequestType.TvShow,
                    Title = model.Title,
                    NotificationType = NotificationType.ItemAddedToFaultQueue
                });
                return Response.AsJson(new JsonResponseModel
                {
                    Result = true,
                    Message = $"{fullShowName} {Resources.UI.Search_SuccessfullyAdded}"
                });
            }

            model.ProviderId = showInfo.externals?.thetvdb ?? 0;

            try
            {
                if (ShouldAutoApprove(RequestType.TvShow))
                {
                    return await SendTv(model, sonarrSettings, existingRequest, fullShowName, settings);
                }
                return await AddRequest(model, settings, $"{fullShowName} {Resources.UI.Search_SuccessfullyAdded}");
            }
            catch (Exception e)
            {
                await FaultQueue.QueueItemAsync(model, showInfo.id.ToString(), RequestType.TvShow, FaultType.RequestFault, e.Message);
                await NotificationService.Publish(new NotificationModel
                {
                    DateTime = DateTime.Now,
                    User = Username,
                    RequestType = RequestType.TvShow,
                    Title = model.Title,
                    NotificationType = NotificationType.ItemAddedToFaultQueue
                });
                Log.Error(e);
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = true,
                        Message = $"{fullShowName} {Resources.UI.Search_SuccessfullyAdded}"
                    });
            }
        }


        private Response GetSeasons()
        {
            var seriesId = (int)Request.Query.tvId;
            var show = TvApi.ShowLookupByTheTvDbId(seriesId);
            var seasons = TvApi.GetSeasons(show.id);
            var model = seasons.Select(x => x.number);
            return Response.AsJson(model);
        }

        private async Task<Response> GetEpisodes()
        {
            var seriesId = (int)Request.Query.tvId;
            var model = await GetEpisodes(seriesId);

            return Response.AsJson(model);
        }

        private async Task<List<EpisodeListViewModel>> GetEpisodes(int providerId)
        {
            var s = await SonarrService.GetSettingsAsync();
            var sonarrEnabled = s.Enabled;
            var allResults = await RequestService.GetAllAsync();

            var seriesTask = Task.Run(
                () =>
                {
                    if (sonarrEnabled)
                    {
                        var allSeries = SonarrApi.GetSeries(s.ApiKey, s.FullUri);
                        var selectedSeries = allSeries.FirstOrDefault(x => x.tvdbId == providerId) ?? new Series();
                        return selectedSeries;
                    }
                    return new Series();
                });

            var model = new List<EpisodeListViewModel>();

            var requests = allResults as RequestedModel[] ?? allResults.ToArray();

            var existingRequest = requests.FirstOrDefault(x => x.Type == RequestType.TvShow && x.TvDbId == providerId.ToString());
            var show = await Task.Run(() => TvApi.ShowLookupByTheTvDbId(providerId));
            var tvMazeEpisodesTask = await Task.Run(() => TvApi.EpisodeLookup(show.id));
            var tvMazeEpisodes = tvMazeEpisodesTask.ToList();

            var sonarrEpisodes = new List<SonarrEpisodes>();
            if (sonarrEnabled)
            {
                var sonarrSeries = await seriesTask;
                var sonarrEp = SonarrApi.GetEpisodes(sonarrSeries.id.ToString(), s.ApiKey, s.FullUri);
                sonarrEpisodes = sonarrEp?.ToList() ?? new List<SonarrEpisodes>();
            }

            var plexSettings = await PlexService.GetSettingsAsync();
            if (plexSettings.Enable)
            {
                var plexCacheTask = await PlexChecker.GetEpisodes(providerId);
                var plexCache = plexCacheTask.ToList();
                foreach (var ep in tvMazeEpisodes)
                {
                    var requested = existingRequest?.Episodes
                                        .Any(episodesModel =>
                                            ep.number == episodesModel.EpisodeNumber &&
                                            ep.season == episodesModel.SeasonNumber) ?? false;

                    var alreadyInPlex = plexCache.Any(x => x.EpisodeNumber == ep.number && x.SeasonNumber == ep.season);
                    var inSonarr =
                        sonarrEpisodes.Any(x => x.seasonNumber == ep.season && x.episodeNumber == ep.number && x.hasFile);

                    model.Add(new EpisodeListViewModel
                    {
                        Id = show.id,
                        SeasonNumber = ep.season,
                        EpisodeNumber = ep.number,
                        Requested = requested || alreadyInPlex || inSonarr,
                        Name = ep.name,
                        EpisodeId = ep.id
                    });
                }
            }
            var embySettings = await EmbySettings.GetSettingsAsync();
            if (embySettings.Enable)
            {
                var embyCacheTask = await EmbyChecker.GetEpisodes(providerId);
                var cache = embyCacheTask.ToList();
                foreach (var ep in tvMazeEpisodes)
                {
                    var requested = existingRequest?.Episodes
                                        .Any(episodesModel =>
                                            ep.number == episodesModel.EpisodeNumber &&
                                            ep.season == episodesModel.SeasonNumber) ?? false;

                    var alreadyInEmby = cache.Any(x => x.EpisodeNumber == ep.number && x.SeasonNumber == ep.season);
                    var inSonarr =
                        sonarrEpisodes.Any(x => x.seasonNumber == ep.season && x.episodeNumber == ep.number && x.hasFile);

                    model.Add(new EpisodeListViewModel
                    {
                        Id = show.id,
                        SeasonNumber = ep.season,
                        EpisodeNumber = ep.number,
                        Requested = requested || alreadyInEmby || inSonarr,
                        Name = ep.name,
                        EpisodeId = ep.id
                    });
                }
            }
            return model;

        }

        private async Task<IEnumerable<EpisodesModel>> GetEpisodeRequestDifference(int showId, RequestedModel model)
        {
            var episodes = await GetEpisodes(showId);
            var availableEpisodes = episodes.Where(x => x.Requested).ToList();
            var available = availableEpisodes.Select(a => new EpisodesModel { EpisodeNumber = a.EpisodeNumber, SeasonNumber = a.SeasonNumber }).ToList();

            var diff = model.Episodes.Except(available);
            return diff;
        }

        private enum ShowSearchType
        {
            Popular,
            Anticipated,
            MostWatched,
            Trending
        }

        private async Task<Response> SendTv(RequestedModel model, Task<SonarrSettings> sonarrSettings, RequestedModel existingRequest, string fullShowName, PlexRequestSettings settings)
        {
            model.Approved = true;
            var s = await sonarrSettings;
            var sender = new TvSenderOld(SonarrApi, SickrageApi, Cache); // TODO put back
            if (s.Enabled)
            {
                var result = await sender.SendToSonarr(s, model);
                if (!string.IsNullOrEmpty(result?.title))
                {
                    if (existingRequest != null)
                    {
                        return await UpdateRequest(model, settings,
                            $"{fullShowName} {Resources.UI.Search_SuccessfullyAdded}");
                    }
                    return
                        await
                            AddRequest(model, settings,
                                $"{fullShowName} {Resources.UI.Search_SuccessfullyAdded}");
                }
                Log.Debug("Error with sending to sonarr.");
                return
                    Response.AsJson(ValidationHelper.SendSonarrError(result?.ErrorMessages ?? new List<string>()));
            }

            var srSettings = SickRageService.GetSettings();
            if (srSettings.Enabled)
            {
                var result = sender.SendToSickRage(srSettings, model);
                if (result?.result == "success")
                {
                    return await AddRequest(model, settings,
                                $"{fullShowName} {Resources.UI.Search_SuccessfullyAdded}");
                }
                return
                    Response.AsJson(new JsonResponseModel
                    {
                        Result = false,
                        Message = result?.message ?? Resources.UI.Search_SickrageError
                    });
            }

            if (!srSettings.Enabled && !s.Enabled)
            {
                return await AddRequest(model, settings, $"{fullShowName} {Resources.UI.Search_SuccessfullyAdded}");
            }

            return
                Response.AsJson(new JsonResponseModel { Result = false, Message = Resources.UI.Search_TvNotSetUp });
        }

        private IEnumerable<EpisodesModel> GetListDifferences(IEnumerable<EpisodesModel> existing, IEnumerable<Models.EpisodesModel> request)
        {
            var newRequest = request
                .Select(r =>
                    new EpisodesModel
                    {
                        SeasonNumber = r.SeasonNumber,
                        EpisodeNumber = r.EpisodeNumber
                    }).ToList();

            return newRequest.Except(existing);
        }
        private Response GetTvPoster(int theTvDbId)
        {
            var result = TvApi.ShowLookupByTheTvDbId(theTvDbId);

            var banner = result.image?.medium;
            if (!string.IsNullOrEmpty(banner))
            {
                banner = banner.Replace("http", "https"); // Always use the Https banners
            }
            return banner;
        }


    }
}
