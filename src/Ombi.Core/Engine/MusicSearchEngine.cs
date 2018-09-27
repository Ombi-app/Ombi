using System;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Rule.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Ombi.Api.Lidarr;
using Ombi.Api.Lidarr.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Engine
{
    public class MusicSearchEngine : BaseMediaEngine, IMusicSearchEngine
    {
        public MusicSearchEngine(IPrincipal identity, IRequestServiceMain service, ILidarrApi lidarrApi, IMapper mapper,
            ILogger<MusicSearchEngine> logger, IRuleEvaluator r, OmbiUserManager um, ICacheService mem, ISettingsService<OmbiSettings> s, IRepository<RequestSubscription> sub,
            ISettingsService<LidarrSettings> lidarrSettings)
            : base(identity, service, r, um, mem, s, sub)
        {
            _lidarrApi = lidarrApi;
            _lidarrSettings = lidarrSettings;
            Mapper = mapper;
            Logger = logger;
        }

        private readonly ILidarrApi _lidarrApi;
        private IMapper Mapper { get; }
        private ILogger Logger { get; }
        private readonly ISettingsService<LidarrSettings> _lidarrSettings;

        /// <summary>
        /// Searches the specified album.
        /// </summary>
        /// <param name="search">The search.</param>
        /// <returns></returns>
        public async Task<IEnumerable<SearchAlbumViewModel>> SearchAlbum(string search)
        {
            var settings = await GetSettings();
            var result = await _lidarrApi.AlbumLookup(search, settings.ApiKey, settings.FullUri);
            var vm = new List<SearchAlbumViewModel>();
            foreach (var r in result)
            {
                vm.Add(await MapIntoAlbumVm(r, settings));
            }

            return vm;
        }

        /// <summary>
        /// Searches the specified artist
        /// </summary>
        /// <param name="search">The search.</param>
        /// <returns></returns>
        public async Task<IEnumerable<SearchArtistViewModel>> SearchArtist(string search)
        {
            var settings = await GetSettings();
            var result = await _lidarrApi.ArtistLookup(search, settings.ApiKey, settings.FullUri);

            var vm = new List<SearchArtistViewModel>();
            foreach (var r in result)
            {
                vm.Add(await MapIntoArtistVm(r));
            }

            return vm;
        }

        /// <summary>
        /// Returns all albums by the specified artist
        /// </summary>
        /// <param name="foreignArtistId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<SearchAlbumViewModel>> GetArtistAlbums(string foreignArtistId)
        {
            var settings = await GetSettings();
            var result = await _lidarrApi.GetAlbumsByArtist(foreignArtistId);
            // We do not want any Singles (This will include EP's)
            var albumsOnly =
                result.Albums.Where(x => !x.Type.Equals("Single", StringComparison.InvariantCultureIgnoreCase));
            var vm = new List<SearchAlbumViewModel>();
            foreach (var album in albumsOnly)
            {
                vm.Add(await MapIntoAlbumVm(album, result.Id, result.ArtistName, settings));
            }
            return vm;
        }

        /// <summary>
        /// Returns the artist that produced the album
        /// </summary>
        /// <param name="foreignArtistId"></param>
        /// <returns></returns>
        public async Task<ArtistResult> GetAlbumArtist(string foreignArtistId)
        {
            var settings = await GetSettings();
            return await _lidarrApi.GetArtistByForeignId(foreignArtistId, settings.ApiKey, settings.FullUri);
        }

        public async Task<ArtistResult> GetArtist(int artistId)
        {
            var settings = await GetSettings();
            return await _lidarrApi.GetArtist(artistId, settings.ApiKey, settings.FullUri);
        }

        private async Task<SearchArtistViewModel> MapIntoArtistVm(ArtistLookup a)
        {
            var vm = new SearchArtistViewModel
            {
                ArtistName = a.artistName,
                ArtistType = a.artistType,
                Banner = a.images?.FirstOrDefault(x => x.coverType.Equals("banner"))?.url,
                Logo = a.images?.FirstOrDefault(x => x.coverType.Equals("logo"))?.url,
                CleanName = a.cleanName,
                Disambiguation = a.disambiguation,
                ForignArtistId = a.foreignArtistId,
                Links = a.links,
                Overview = a.overview,
            };

            var poster = a.images?.FirstOrDefault(x => x.coverType.Equals("poaster"));
            if (poster == null)
            {
                vm.Poster = a.remotePoster;
            }


            await Rules.StartSpecificRules(vm, SpecificRules.LidarrArtist);

            return vm;
        }

        private async Task<SearchAlbumViewModel> MapIntoAlbumVm(AlbumLookup a, LidarrSettings settings)
        {
            var vm = new SearchAlbumViewModel
            {
                ForeignAlbumId = a.foreignAlbumId,
                Monitored = a.monitored,
                Rating = a.ratings?.value ?? 0m,
                ReleaseDate = a.releaseDate,
                Title = a.title,
                Disk = a.images?.FirstOrDefault(x => x.coverType.Equals("disc"))?.url
            };
            if (a.artistId > 0)
            {
                //TODO THEY HAVE FIXED THIS IN DEV
                // The JSON is different for some stupid reason
                // Need to lookup the artist now and all the images -.-"
                var artist = await _lidarrApi.GetArtist(a.artistId, settings.ApiKey, settings.FullUri);
                vm.ArtistName = artist.artistName;
                vm.ForeignArtistId = artist.foreignArtistId;
            }
            else
            {
                vm.ForeignArtistId = a.artist?.foreignArtistId;
                vm.ArtistName = a.artist?.artistName;
            }

            vm.Cover = a.images?.FirstOrDefault(x => x.coverType.Equals("cover"))?.url;
            if (vm.Cover.IsNullOrEmpty())
            {
                vm.Cover = a.remoteCover;
            }

            await Rules.StartSpecificRules(vm, SpecificRules.LidarrAlbum);

            await RunSearchRules(vm);

            return vm;
        }

        private async Task<SearchAlbumViewModel> MapIntoAlbumVm(Album a, string artistId, string artistName, LidarrSettings settings)
        {
            var fullAlbum = await _lidarrApi.GetAlbumByForeignId(a.Id, settings.ApiKey, settings.FullUri);
            var vm = new SearchAlbumViewModel
            {
                ForeignAlbumId = a.Id,
                Monitored = fullAlbum.monitored,
                Rating = fullAlbum.ratings?.value ?? 0m,
                ReleaseDate = fullAlbum.releaseDate,
                Title = a.Title,
                Disk = fullAlbum.images?.FirstOrDefault(x => x.coverType.Equals("disc"))?.url,
                ForeignArtistId = artistId,
                ArtistName = artistName,
                Cover = fullAlbum.images?.FirstOrDefault(x => x.coverType.Equals("cover"))?.url
            };

            if (vm.Cover.IsNullOrEmpty())
            {
                vm.Cover = fullAlbum.remoteCover;
            }

            await Rules.StartSpecificRules(vm, SpecificRules.LidarrAlbum);

            await RunSearchRules(vm);

            return vm;
        }

        private LidarrSettings _settings;
        private async Task<LidarrSettings> GetSettings()
        {
            return _settings ?? (_settings = await _lidarrSettings.GetSettingsAsync());
        }
    }
}