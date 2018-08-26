using System;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Api.Lidarr;
using Ombi.Api.Lidarr.Models;
using Ombi.Api.Radarr;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities.Requests;
using Serilog;

namespace Ombi.Core.Senders
{
    public class MusicSender : IMusicSender
    {
        public MusicSender(ISettingsService<LidarrSettings> lidarr, ILidarrApi lidarrApi)
        {
            _lidarrSettings = lidarr;
            _lidarrApi = lidarrApi;
        }

        private readonly ISettingsService<LidarrSettings> _lidarrSettings;
        private readonly ILidarrApi _lidarrApi;

        public async Task<SenderResult> Send(AlbumRequest model)
        {
            var settings = await _lidarrSettings.GetSettingsAsync();
            if (settings.Enabled)
            {
                return await SendToLidarr(model, settings);
            }

            return new SenderResult { Success = false, Sent = false, Message = "Lidarr is not enabled" };
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
                // Create artist
                var newArtist = new ArtistAdd
                {
                    foreignArtistId = model.ForeignArtistId,
                    addOptions = new Addoptions
                    {
                        monitored = true,
                        searchForMissingAlbums = false,
                        selectedOption = 6 // None
                    },
                    added = DateTime.Now,
                    monitored = true,
                    albumFolder = settings.AlbumFolder,
                    artistName = model.ArtistName,
                    cleanName = model.ArtistName.ToLowerInvariant().RemoveSpaces(),
                    images = new Image[] { },
                    languageProfileId = settings.LanguageProfileId,
                    links = new Link[] {},
                    metadataProfileId = settings.MetadataProfileId,
                    qualityProfileId = qualityToUse,
                    rootFolderPath = rootFolderPath,
                };

                var result = await _lidarrApi.AddArtist(newArtist, settings.ApiKey, settings.FullUri);
                if (result != null && result.id > 0)
                {
                    // Setup the albums
                    await SetupAlbum(model, result, settings);
                }
            }
            else
            {
                await SetupAlbum(model, artist, settings);
            }

            return new SenderResult { Success = false, Sent = false, Message = "Album is already monitored" };
        }

        private async Task<SenderResult> SetupAlbum(AlbumRequest model, ArtistResult artist, LidarrSettings settings)
        {
            // Get the album id
            var albums = await _lidarrApi.GetAllAlbumsByArtistId(artist.id, settings.ApiKey, settings.FullUri);
            // Get the album we want.
            var album = albums.FirstOrDefault(x =>
                x.foreignAlbumId.Equals(model.ForeignAlbumId, StringComparison.InvariantCultureIgnoreCase));
            if (album == null)
            {
                return new SenderResult { Message = "Could not find album in Lidarr", Sent = false, Success = false };
            }

            var result = await _lidarrApi.MontiorAlbum(album.id, settings.ApiKey, settings.FullUri);
            if (result.monitored)
            {
                return new SenderResult {Message = "Album has been requested!", Sent = true, Success = true};
            }
            return new SenderResult { Message = "Could not set album to monitored", Sent = false, Success = false };
        }
    }
}