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
        public async Task<IEnumerable<AlbumLookup>> SearchAlbum(string search)
        {
            var settings = await GetSettings();
            var result = await _lidarrApi.AlbumLookup(search, settings.ApiKey, settings.FullUri);

            return result;
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
                vm.Add(MapIntoArtistVm(r));
            }

            return vm;
        }

        private SearchArtistViewModel MapIntoArtistVm(ArtistLookup a)
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
            
            return vm;
        }

        /// <summary>
        /// Returns all albums by the specified artist
        /// </summary>
        /// <param name="artistId"></param>
        /// <returns></returns>
        public async Task<ArtistResult> GetArtistAlbums(string foreignArtistId)
        {
            var settings = await GetSettings();
            return await _lidarrApi.GetArtistByForignId(foreignArtistId, settings.ApiKey, settings.FullUri);
        }

        /// <summary>
        /// Returns the artist that produced the album
        /// </summary>
        /// <param name="foreignArtistId"></param>
        /// <returns></returns>
        public async Task<ArtistResult> GetAlbumArtist(string foreignArtistId)
        {
            var settings = await GetSettings();
            return await _lidarrApi.GetArtistByForignId(foreignArtistId, settings.ApiKey, settings.FullUri);
        }

        public async Task<ArtistResult> GetArtist(int artistId)
        {
            var settings = await GetSettings();
            return await _lidarrApi.GetArtist(artistId, settings.ApiKey, settings.FullUri);
        }


        private LidarrSettings _settings;
        private async Task<LidarrSettings> GetSettings()
        {
            return _settings ?? (_settings = await _lidarrSettings.GetSettingsAsync());
        }
    }
}