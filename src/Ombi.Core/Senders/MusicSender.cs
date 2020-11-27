using System;
using System.Linq;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Ombi.Api.Lidarr;
using Ombi.Api.Lidarr.Models;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Ombi.Core.Senders
{
    public class MusicSender : IMusicSender
    {
        public MusicSender(ISettingsService<LidarrSettings> lidarr, ILidarrApi lidarrApi, ILogger<MusicSender> log,
            IRepository<RequestQueue> requestQueue, INotificationHelper notify)
        {
            _lidarrSettings = lidarr;
            _lidarrApi = lidarrApi;
            _log = log;
            _requestQueueRepository = requestQueue;
            _notificationHelper = notify;
        }

        private readonly ISettingsService<LidarrSettings> _lidarrSettings;
        private readonly ILidarrApi _lidarrApi;
        private readonly ILogger _log;
        private readonly IRepository<RequestQueue> _requestQueueRepository;
        private readonly INotificationHelper _notificationHelper;

        public async Task<SenderResult> Send(AlbumRequest model)
        {
            try
            {
                var settings = await _lidarrSettings.GetSettingsAsync();
                if (settings.Enabled)
                {
                    return await SendToLidarr(model, settings);
                }

                return new SenderResult { Success = false, Sent = false, Message = "Lidarr is not enabled" };
            }
            catch (Exception e)
            {
                _log.LogError(e, "Exception thrown when sending a music to DVR app, added to the request queue");
                var existingQueue = await _requestQueueRepository.FirstOrDefaultAsync(x => x.RequestId == model.Id);
                if (existingQueue != null)
                {
                    existingQueue.RetryCount++;
                    existingQueue.Error = e.Message;
                    await _requestQueueRepository.SaveChangesAsync();
                }
                else
                {
                    await _requestQueueRepository.Add(new RequestQueue
                    {
                        Dts = DateTime.UtcNow,
                        Error = e.Message,
                        RequestId = model.Id,
                        Type = RequestType.Album,
                        RetryCount = 0
                    });
                    await _notificationHelper.Notify(model, NotificationType.ItemAddedToFaultQueue);
                }
            }


            return new SenderResult { Success = false, Sent = false, Message = "Something went wrong!" };
        }

        private async Task<SenderResult> SendToLidarr(AlbumRequest model, LidarrSettings settings)
        {
            var qualityToUse = int.Parse(settings.DefaultQualityProfile);
            //if (model.QualityOverride > 0)
            //{
            //    qualityToUse = model.QualityOverride;
            //}

            var rootFolderPath = /*model.RootPathOverride <= 0 ?*/ settings.DefaultRootPath /*: await RadarrRootPath(model.RootPathOverride, settings)*/;

            // Need to get the artist
            var artist = await _lidarrApi.GetArtistByForeignId(model.ForeignArtistId, settings.ApiKey, settings.FullUri);

            if (artist == null || artist.id <= 0)
            {
                EnsureArg.IsNotNullOrEmpty(model.ForeignArtistId, nameof(model.ForeignArtistId));
                EnsureArg.IsNotNullOrEmpty(model.ForeignAlbumId, nameof(model.ForeignAlbumId));
                EnsureArg.IsNotNullOrEmpty(model.ArtistName, nameof(model.ArtistName));
                EnsureArg.IsNotNullOrEmpty(rootFolderPath, nameof(rootFolderPath));

                // Create artist
                var newArtist = new ArtistAdd
                {
                    foreignArtistId = model.ForeignArtistId,
                    addOptions = new Addoptions
                    {
                        monitored = true,
                        monitor = MonitorTypes.None,
                        searchForMissingAlbums = false,
                        AlbumsToMonitor = new[] {model.ForeignAlbumId}
                    },
                    added = DateTime.Now,
                    monitored = true,
                    albumFolder = settings.AlbumFolder,
                    artistName = model.ArtistName,
                    cleanName = model.ArtistName.ToLowerInvariant().RemoveSpaces(),
                    images = new Image[] { },
                    links = new Link[] {},
                    metadataProfileId = settings.MetadataProfileId,
                    qualityProfileId = qualityToUse,
                    rootFolderPath = rootFolderPath,
                };

                var result = await _lidarrApi.AddArtist(newArtist, settings.ApiKey, settings.FullUri);
                if (result != null && result.id > 0)
                {
                    // Search for it
                    if (!settings.AddOnly)
                    {
                        // get the album
                        var album = await _lidarrApi.GetAllAlbumsByArtistId(result.id, settings.ApiKey, settings.FullUri);

                        var albumToSearch = album.FirstOrDefault(x =>
                            x.foreignAlbumId == model.ForeignAlbumId);
                        var maxRetryCount = 10; // 5 seconds
                        var currentRetry = 0;
                        while (albumToSearch != null)
                        {
                            if (currentRetry >= maxRetryCount)
                            {
                                break;
                            }
                            currentRetry++;
                            await Task.Delay(500);
                            album = await _lidarrApi.GetAllAlbumsByArtistId(result.id, settings.ApiKey, settings.FullUri);
                            albumToSearch = album.FirstOrDefault(x =>
                                x.foreignAlbumId == model.ForeignAlbumId);
                        }


                        if (albumToSearch != null)
                        {
                            await _lidarrApi.AlbumSearch(new[] {albumToSearch.id}, settings.ApiKey, settings.FullUri);
                        }
                    }
                    return new SenderResult { Message = "Album has been requested!", Sent = true, Success = true };
                }
            }
            else
            {
                SenderResult result = await SetupAlbum(model, artist, settings);
                return result;
            }

            return new SenderResult { Success = false, Sent = false, Message = "Album is already monitored" };
        }

        private async Task<SenderResult> SetupAlbum(AlbumRequest model, ArtistResult artist, LidarrSettings settings)
        {
            // Get the album id
            var albums = await _lidarrApi.GetAllAlbumsByArtistId(artist.id, settings.ApiKey, settings.FullUri);
            var album = albums.FirstOrDefault(x =>
                x.foreignAlbumId == model.ForeignAlbumId);
            var maxRetryCount = 10; // 5 seconds
            var currentRetry = 0;
            while (!albums.Any() || album == null)
            {
                if (currentRetry >= maxRetryCount)
                {
                    break;
                }
                currentRetry++;
                await Task.Delay(500);
                albums = await _lidarrApi.GetAllAlbumsByArtistId(artist.id, settings.ApiKey, settings.FullUri);
                album = albums.FirstOrDefault(x =>
                    x.foreignAlbumId == model.ForeignAlbumId);
            }
            // Get the album we want.

            if (album == null)
            {
                return new SenderResult { Message = "Could not find album in Lidarr", Sent = false, Success = false };
            }

            var result = await _lidarrApi.MontiorAlbum(album.id, settings.ApiKey, settings.FullUri);
            if (!settings.AddOnly)
            {
                await _lidarrApi.AlbumSearch(new[] {result.id}, settings.ApiKey, settings.FullUri);
            }
            if (result.monitored)
            {
                return new SenderResult { Message = "Album has been requested!", Sent = true, Success = true};
            }
            return new SenderResult { Message = "Could not set album to monitored", Sent = false, Success = false };
        }
    }
}