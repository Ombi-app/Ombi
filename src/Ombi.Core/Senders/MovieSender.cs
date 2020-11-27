using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.CouchPotato;
using Ombi.Api.DogNzb.Models;
using Ombi.Api.Radarr;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities.Requests;
using Ombi.Api.DogNzb;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using System.Collections.Generic;
using Ombi.Api.Radarr.Models;

namespace Ombi.Core.Senders
{
    public class MovieSender : IMovieSender
    {
        public MovieSender(ISettingsService<RadarrSettings> radarrSettings, IRadarrApi api, ILogger<MovieSender> log,
            ISettingsService<DogNzbSettings> dogSettings, IDogNzbApi dogApi, ISettingsService<CouchPotatoSettings> cpSettings,
            ICouchPotatoApi cpApi, IRepository<UserQualityProfiles> userProfiles, IRepository<RequestQueue> requestQueue, INotificationHelper notify,
            IRadarrV3Api radarrV3Api)
        {
            _radarrSettings = radarrSettings;
            _radarrV2Api = api;
            _log = log;
            _dogNzbSettings = dogSettings;
            _dogNzbApi = dogApi;
            _couchPotatoSettings = cpSettings;
            _couchPotatoApi = cpApi;
            _userProfiles = userProfiles;
            _requestQueuRepository = requestQueue;
            _notificationHelper = notify;
            _radarrV3Api = radarrV3Api;
        }

        private readonly ISettingsService<RadarrSettings> _radarrSettings;
        private readonly IRadarrApi _radarrV2Api;
        private readonly ILogger<MovieSender> _log;
        private readonly IDogNzbApi _dogNzbApi;
        private readonly ISettingsService<DogNzbSettings> _dogNzbSettings;
        private readonly ISettingsService<CouchPotatoSettings> _couchPotatoSettings;
        private readonly ICouchPotatoApi _couchPotatoApi;
        private readonly IRepository<UserQualityProfiles> _userProfiles;
        private readonly IRepository<RequestQueue> _requestQueuRepository;
        private readonly INotificationHelper _notificationHelper;
        private readonly IRadarrV3Api _radarrV3Api;

        public async Task<SenderResult> Send(MovieRequests model)
        {
            try
            {
                var cpSettings = await _couchPotatoSettings.GetSettingsAsync();
                //var watcherSettings = await WatcherSettings.GetSettingsAsync();
                var radarrSettings = await _radarrSettings.GetSettingsAsync();
                if (radarrSettings.Enabled)
                {
                    return await SendToRadarr(model, radarrSettings);
                }

                var dogSettings = await _dogNzbSettings.GetSettingsAsync();
                if (dogSettings.Enabled)
                {
                    await SendToDogNzb(model, dogSettings);
                    return new SenderResult
                    {
                        Success = true,
                        Sent = true,
                    };
                }

                if (cpSettings.Enabled)
                {
                    return await SendToCp(model, cpSettings, cpSettings.DefaultProfileId);
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error when sending movie to DVR app, added to the request queue");

                // Check if already in request quee
                var existingQueue = await _requestQueuRepository.FirstOrDefaultAsync(x => x.RequestId == model.Id);
                if (existingQueue != null)
                {
                    existingQueue.RetryCount++;
                    existingQueue.Error = e.Message;
                    await _requestQueuRepository.SaveChangesAsync();
                }
                else
                {
                    await _requestQueuRepository.Add(new RequestQueue
                    {
                        Dts = DateTime.UtcNow,
                        Error = e.Message,
                        RequestId = model.Id,
                        Type = RequestType.Movie,
                        RetryCount = 0
                    });
                    await _notificationHelper.Notify(model, NotificationType.ItemAddedToFaultQueue);
                }
            }

            return new SenderResult
            {
                Success = true,
                Sent = false,
            };
        }

        private async Task<SenderResult> SendToCp(FullBaseRequest model, CouchPotatoSettings cpSettings, string cpSettingsDefaultProfileId)
        {
            var result = await _couchPotatoApi.AddMovie(model.ImdbId, cpSettings.ApiKey, model.Title, cpSettings.FullUri, cpSettingsDefaultProfileId);
            return new SenderResult { Success = result, Sent = true };
        }

        private async Task<DogNzbMovieAddResult> SendToDogNzb(FullBaseRequest model, DogNzbSettings settings)
        {
            var id = model.ImdbId;
            return await _dogNzbApi.AddMovie(settings.ApiKey, id);
        }

        private async Task<SenderResult> SendToRadarr(MovieRequests model, RadarrSettings settings)
        {
            var v3 = settings.V3;
            var qualityToUse = int.Parse(settings.DefaultQualityProfile);

            var rootFolderPath = settings.DefaultRootPath;

            var profiles = await _userProfiles.GetAll().FirstOrDefaultAsync(x => x.UserId == model.RequestedUserId);
            if (profiles != null)
            {
                if (profiles.RadarrRootPath > 0)
                {
                    var tempPath = await RadarrRootPath(profiles.RadarrRootPath, settings);
                    if (tempPath.HasValue())
                    {
                        rootFolderPath = tempPath;
                    }
                }
                if (profiles.RadarrQualityProfile > 0)
                {
                    qualityToUse = profiles.RadarrQualityProfile;
                }
            }

            // Overrides on the request take priority
            if (model.QualityOverride > 0)
            {
                qualityToUse = model.QualityOverride;
            }
            if (model.RootPathOverride > 0)
            {
                rootFolderPath = await RadarrRootPath(model.RootPathOverride, settings);
            }

            List<MovieResponse> movies;
            // Check if the movie already exists? Since it could be unmonitored
            if (settings.V3)
            {
                movies = await _radarrV3Api.GetMovies(settings.ApiKey, settings.FullUri);
            }
            else
            {
                movies = await _radarrV2Api.GetMovies(settings.ApiKey, settings.FullUri);
            }
            var existingMovie = movies.FirstOrDefault(x => x.tmdbId == model.TheMovieDbId);
            if (existingMovie == null)
            {
                RadarrAddMovie result;
                if (v3)
                {
                    result = await _radarrV3Api.AddMovie(model.TheMovieDbId, model.Title, model.ReleaseDate.Year,
                        qualityToUse, rootFolderPath, settings.ApiKey, settings.FullUri, !settings.AddOnly,
                        settings.MinimumAvailability);
                }
                else
                {
                    result = await _radarrV2Api.AddMovie(model.TheMovieDbId, model.Title, model.ReleaseDate.Year,
                        qualityToUse, rootFolderPath, settings.ApiKey, settings.FullUri, !settings.AddOnly,
                        settings.MinimumAvailability);
                }
                if (!string.IsNullOrEmpty(result.Error?.message))
                {
                    _log.LogError(LoggingEvents.RadarrCacher, result.Error.message);
                    return new SenderResult { Success = false, Message = result.Error.message, Sent = false };
                }
                if (!string.IsNullOrEmpty(result.title))
                {
                    return new SenderResult { Success = true, Sent = false };
                }
                return new SenderResult { Success = true, Sent = false };
            }
            // We have the movie, check if we can request it or change the status
            if (!existingMovie.monitored)
            {
                // let's set it to monitored and search for it
                existingMovie.monitored = true;
                if (v3)
                {
                    await _radarrV3Api.UpdateMovie(existingMovie, settings.ApiKey, settings.FullUri);
                    // Search for it
                    if (!settings.AddOnly)
                    {
                        await _radarrV3Api.MovieSearch(new[] { existingMovie.id }, settings.ApiKey, settings.FullUri);
                    }
                }
                else
                {
                    await _radarrV2Api.UpdateMovie(existingMovie, settings.ApiKey, settings.FullUri);
                    // Search for it
                    if (!settings.AddOnly)
                    {
                        await _radarrV2Api.MovieSearch(new[] { existingMovie.id }, settings.ApiKey, settings.FullUri);
                    }
                }

                return new SenderResult { Success = true, Sent = true };
            }

            return new SenderResult { Success = false, Sent = false, Message = "Movie is already monitored" };
        }

        private async Task<string> RadarrRootPath(int overrideId, RadarrSettings settings)
        {
            if (settings.V3)
            {
                var paths = await _radarrV3Api.GetRootFolders(settings.ApiKey, settings.FullUri);
                var selectedPath = paths.FirstOrDefault(x => x.id == overrideId);
                return selectedPath?.path ?? string.Empty;
            }
            else
            {
                var paths = await _radarrV2Api.GetRootFolders(settings.ApiKey, settings.FullUri);
                var selectedPath = paths.FirstOrDefault(x => x.id == overrideId);
                return selectedPath?.path ?? string.Empty;
            }
        }
    }
}