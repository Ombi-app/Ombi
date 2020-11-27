using System;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Api.FanartTv;
using Ombi.Helpers;
using Ombi.Store.Repository;

namespace Ombi.Core
{
    public class ImageService : IImageService
    {
        private readonly IApplicationConfigRepository _configRepository;
        private readonly IFanartTvApi _fanartTvApi;
        private readonly ICacheService _cache;

        public ImageService(IApplicationConfigRepository configRepository, IFanartTvApi fanartTvApi,
            ICacheService cache)
        {
            _configRepository = configRepository;
            _fanartTvApi = fanartTvApi;
            _cache = cache;
        }

        public async Task<string> GetTvBackground(string tvdbId)
        {
            var key = await _cache.GetOrAdd(CacheKeys.FanartTv, async () => await _configRepository.GetAsync(Store.Entities.ConfigurationTypes.FanartTv), DateTime.Now.AddDays(1));
            var images = await _cache.GetOrAdd($"{CacheKeys.FanartTv}tv{tvdbId}", async () => await _fanartTvApi.GetTvImages(int.Parse(tvdbId), key.Value), DateTime.Now.AddDays(1));

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
    }
}