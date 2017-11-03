using Microsoft.AspNetCore.Mvc;
using Ombi.Api.FanartTv;
using Ombi.Store.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Ombi.Config;
using Ombi.Helpers;

namespace Ombi.Controllers
{
    [ApiV1]
    [Produces("application/json")]
    public class ImagesController : Controller
    {
        public ImagesController(IFanartTvApi api, IApplicationConfigRepository config,
            IOptions<LandingPageBackground> options, IMemoryCache c)
        {
            Api = api;
            Config = config;
            Options = options.Value;
            _cache = c;
        }

        private IFanartTvApi Api { get; }
        private IApplicationConfigRepository Config { get; }
        private LandingPageBackground Options { get; }
        private readonly IMemoryCache _cache;
        
        [HttpGet("tv/{tvdbid}")]
        public async Task<string> GetTvBanner(int tvdbid)
        {
            var key = await _cache.GetOrCreateAsync(CacheKeys.FanartTv, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromDays(1);
                return await Config.Get(Store.Entities.ConfigurationTypes.FanartTv);
            });

            var images = await Api.GetTvImages(tvdbid, key.Value);
            if (images.tvbanner != null)
            {
                return images.tvbanner.FirstOrDefault()?.url ?? string.Empty;
            }
            if (images.showbackground != null)
            {
                return images.showbackground.FirstOrDefault()?.url ?? string.Empty;
            }
            return string.Empty;
        }

        [HttpGet("background")]
        public async Task<object> GetBackgroundImage()
        {
            var moviesArray = Options.Movies;
            var tvArray = Options.TvShows;

            var rand = new Random();
            var movieUrl = string.Empty;
            var tvUrl = string.Empty;

            var key = await _cache.GetOrCreateAsync(CacheKeys.FanartTv, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromDays(1);
                return await Config.Get(Store.Entities.ConfigurationTypes.FanartTv);
            });

            if (moviesArray.Any())
            {
                var item = rand.Next(moviesArray.Length);
                var result = await Api.GetMovieImages(moviesArray[item], key.Value);

                while (!result.moviebackground.Any())
                {
                    result = await Api.GetMovieImages(moviesArray[item], key.Value);
                }

                movieUrl = result.moviebackground[0].url;
            }
            if(tvArray.Any())
            {
                var item = rand.Next(tvArray.Length);
                var result = await Api.GetTvImages(tvArray[item], key.Value);

                while (!result.showbackground.Any())
                {
                    result = await Api.GetTvImages(tvArray[item], key.Value);
                }

                tvUrl = result.showbackground[0].url;
            }

            if (!string.IsNullOrEmpty(movieUrl) && !string.IsNullOrEmpty(tvUrl))
            {
                var result = rand.Next(2);
                if (result == 0) return new { url = movieUrl }; 
                if (result == 1) return new { url = tvUrl }; 
            }

            if (!string.IsNullOrEmpty(movieUrl))
            {
                return new { url = movieUrl };
            }
            return new { url = tvUrl };
        }
    }
}
