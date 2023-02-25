using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ombi.Api.FanartTv;
using Ombi.Api.TheMovieDb;
using Ombi.Core.Helpers;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Repository;

namespace Ombi.Core
{
    public class ImageService : IImageService
    {
        private readonly IApplicationConfigRepository _configRepository;
        private readonly IFanartTvApi _fanartTvApi;
        private readonly ICacheService _cache;
        private readonly IMovieDbApi _movieDbApi;
        private readonly ICurrentUser _user;
        private readonly ISettingsService<OmbiSettings> _ombiSettings;

        public ImageService(IApplicationConfigRepository configRepository, IFanartTvApi fanartTvApi,
            ICacheService cache, IMovieDbApi movieDbApi, ICurrentUser user, ISettingsService<OmbiSettings> ombiSettings)
        {
            _configRepository = configRepository;
            _fanartTvApi = fanartTvApi;
            _cache = cache;
            _movieDbApi = movieDbApi;
            _user = user;
            _ombiSettings = ombiSettings;
        }

        public async Task<string> GetTvBackground(string tvdbId)
        {
            var key = await _cache.GetOrAddAsync(CacheKeys.FanartTv, () => _configRepository.GetAsync(Store.Entities.ConfigurationTypes.FanartTv), DateTimeOffset.Now.AddDays(1));
            var images = await _cache.GetOrAddAsync($"{CacheKeys.FanartTv}tv{tvdbId}", () => _fanartTvApi.GetTvImages(int.Parse(tvdbId), key.Value), DateTimeOffset.Now.AddDays(1));

            if (images == null)
            {
                return string.Empty;
            }

            if (images.showbackground?.Any() ?? false)
            {
                var enImage = images.showbackground.Where(x => x.lang == "en").OrderByDescending(x => x.likes).Select(x => x.url).FirstOrDefault();
                if (enImage == null)
                {
                    return images.showbackground.OrderByDescending(x => x.likes).Select(x => x.url).FirstOrDefault();
                }
                return enImage;
            }

            return string.Empty;
        }

        public async Task<string> GetTmdbTvBackground(string id, CancellationToken token)
        {
            var images = await _cache.GetOrAddAsync($"{CacheKeys.TmdbImages}tv{id}", () => _movieDbApi.GetTvImages(id, token), DateTimeOffset.Now.AddDays(1));

            if (images?.backdrops?.Any() ?? false)
            {
                return images.backdrops.Select(x => x.file_path).FirstOrDefault();
            }
            if (images?.posters?.Any() ?? false)
            {
                return images.posters.Select(x => x.file_path).FirstOrDefault();
            }

            return string.Empty;
        }

        public async Task<string> GetTmdbTvPoster(string tmdbId, CancellationToken token)
        {
            var images = await _cache.GetOrAddAsync($"{CacheKeys.TmdbImages}tv{tmdbId}", () => _movieDbApi.GetTvImages(tmdbId, token), DateTimeOffset.Now.AddDays(1));

            if (images?.posters?.Any() ?? false)
            {
                var lang = await DefaultLanguageCode();
                var langImage = images.posters.Where(x => lang.Equals(x.iso_639_1, StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(x => x.vote_count);
                if (langImage.Any())
                {
                    return langImage.Select(x => x.file_path).First();
                }
                else
                {
                    return images.posters.Select(x => x.file_path).First();
                }
            }

            if (images?.backdrops?.Any() ?? false)
            {
                return images.backdrops.Select(x => x.file_path).FirstOrDefault();
            }
            return string.Empty;
        }

        protected async Task<string> DefaultLanguageCode()
        {
            var user = await _user.GetUser();
            if (user == null)
            {
                return "en";
            }

            if (string.IsNullOrEmpty(user.Language))
            {
                var s = await GetOmbiSettings();
                return s.DefaultLanguageCode;
            }

            return user.Language;
        }

        private OmbiSettings ombiSettings;
        protected async Task<OmbiSettings> GetOmbiSettings()
        {
            return ombiSettings ?? (ombiSettings = await _ombiSettings.GetSettingsAsync());
        }
    }
}