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
            IOptions<LandingPageBackground> options, ICacheService c)
        {
            Api = api;
            Config = config;
            Options = options.Value;
            _cache = c;
        }

        private IFanartTvApi Api { get; }
        private IApplicationConfigRepository Config { get; }
        private LandingPageBackground Options { get; }
        private readonly ICacheService _cache;
        
        [HttpGet("tv/{tvdbid}")]
        public async Task<string> GetTvBanner(int tvdbid)
        {
            var key = await _cache.GetOrAdd(CacheKeys.FanartTv, async () => await Config.Get(Store.Entities.ConfigurationTypes.FanartTv), DateTime.Now.AddDays(1));

            var images = await Api.GetTvImages(tvdbid, key.Value);
            if (images.tvbanner != null)
            {
                return images.tvbanner.FirstOrDefault()?.url ?? string.Empty;
            }
            return string.Empty;
        }

        [HttpGet("background/tv/{tvdbid}")]
        public async Task<string> GetTvBackground(int tvdbid)
        {
            var key = await _cache.GetOrAdd(CacheKeys.FanartTv, async () => await Config.Get(Store.Entities.ConfigurationTypes.FanartTv), DateTime.Now.AddDays(1));

            var images = await Api.GetTvImages(tvdbid, key.Value);
            if (images.showbackground != null)
            {
                return images.showbackground.FirstOrDefault()?.url ?? string.Empty;
            }
            return string.Empty;
        }

        [HttpGet("poster/tv/{tvdbid}")]
        public async Task<string> GetTvPoster(int tvdbid)
        {
            var key = await _cache.GetOrAdd(CacheKeys.FanartTv, async () => await Config.Get(Store.Entities.ConfigurationTypes.FanartTv), DateTime.Now.AddDays(1));

            var images = await Api.GetTvImages(tvdbid, key.Value);
            if (images.tvposter != null)
            {
                return images.tvposter.FirstOrDefault()?.url ?? string.Empty;
            }
            return string.Empty;
        }

        [HttpGet("movies/{themoviedb}")]
        public async Task<string> GetMovieBackground(int themoviedb)
        {
            var key = await _cache.GetOrAdd(CacheKeys.FanartTv, async () => await Config.Get(Store.Entities.ConfigurationTypes.FanartTv), DateTime.Now.AddDays(1));

            var images = await Api.GetMovieImages(themoviedb, key.Value);
            if (images.moviebackground != null)
            {
                return images.moviebackground.FirstOrDefault()?.url ?? string.Empty;
            }
            return string.Empty;
        }

        [HttpGet("poster/movies/{themoviedb}")]
        public async Task<string> GetMoviePoster(int themoviedb)
        {
            var key = await _cache.GetOrAdd(CacheKeys.FanartTv, async () => await Config.Get(Store.Entities.ConfigurationTypes.FanartTv), DateTime.Now.AddDays(1));

            var images = await Api.GetMovieImages(themoviedb, key.Value);
            if (images.movieposter != null)
            {
                return images.movieposter.FirstOrDefault()?.url ?? string.Empty;
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

            var key = await _cache.GetOrAdd(CacheKeys.FanartTv, async () => await Config.Get(Store.Entities.ConfigurationTypes.FanartTv), DateTime.Now.AddDays(1));

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
