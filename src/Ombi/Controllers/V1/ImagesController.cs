using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Ombi.Api.FanartTv;
using Ombi.Api.TheMovieDb;
using Ombi.Config;
using Ombi.Core;
using Ombi.Core.Engine.Interfaces;
using Ombi.Helpers;
using Ombi.Store.Repository;

namespace Ombi.Controllers.V1
{
    [ApiV1]
    [Produces("application/json")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        public ImagesController(IFanartTvApi fanartTvApi, IMovieDbApi movieDbApi, IApplicationConfigRepository config,
            IOptions<LandingPageBackground> options, ICacheService c, IImageService imageService,
            IMovieEngineV2 movieEngineV2, ITVSearchEngineV2 tVSearchEngineV2)
        {
            FanartTvApi = fanartTvApi;
            _movieDbApi = movieDbApi;
            Config = config;
            Options = options.Value;
            _cache = c;
            _imageService = imageService;
            _movieEngineV2 = movieEngineV2;
            _tvSearchEngineV2 = tVSearchEngineV2;
        }

        private IFanartTvApi FanartTvApi { get; }
        private IApplicationConfigRepository Config { get; }
        private LandingPageBackground Options { get; }

        private readonly IMovieDbApi _movieDbApi;
        private readonly ICacheService _cache;
        private readonly IImageService _imageService;
        private readonly IMovieEngineV2 _movieEngineV2;
        private readonly ITVSearchEngineV2 _tvSearchEngineV2;

        [HttpGet("tv/{tvdbid}")]
        public async Task<string> GetTvBanner(int tvdbid)
        {
            if (tvdbid <= 0)
            {
                return string.Empty;
            }
            var key = await _cache.GetOrAddAsync(CacheKeys.FanartTv, () => Config.GetAsync(Store.Entities.ConfigurationTypes.FanartTv), DateTimeOffset.Now.AddDays(1));

            var images = await _cache.GetOrAddAsync($"{CacheKeys.FanartTv}tv{tvdbid}", () => FanartTvApi.GetTvImages(tvdbid, key.Value), DateTimeOffset.Now.AddDays(1));
            if (images == null)
            {
                return string.Empty;
            }
            if (images.tvbanner != null)
            {
                var enImage = images.tvbanner.Where(x => x.lang == "en").OrderByDescending(x => x.likes).Select(x => x.url).FirstOrDefault();
                if (enImage == null)
                {
                    return images.tvbanner.OrderByDescending(x => x.likes).Select(x => x.url).FirstOrDefault();
                }
            }
            if (images.seasonposter != null)
            {
                return images.seasonposter.FirstOrDefault()?.url ?? string.Empty;
            }
            return string.Empty;
        }

        [HttpGet("poster")]
        public async Task<string> GetRandomPoster()
        {
            var key = await _cache.GetOrAddAsync(CacheKeys.FanartTv, () => Config.GetAsync(Store.Entities.ConfigurationTypes.FanartTv), DateTimeOffset.Now.AddDays(1));
            var rand = new Random();
            var val = rand.Next(1, 3);
            if (val == 1)
            {
                var movies = (await _movieEngineV2.PopularMovies(0, 10, HttpContext.RequestAborted, "en")).ToArray();
                var selectedMovieIndex = rand.Next(movies.Count());
                var movie = movies[selectedMovieIndex];

                var images = await _cache.GetOrAddAsync($"{CacheKeys.FanartTv}movie{movie.Id}", () => FanartTvApi.GetMovieImages(movie.Id.ToString(), key.Value), DateTimeOffset.Now.AddDays(1));
                if (images == null)
                {
                    return string.Empty;
                }

                if (images.movieposter?.Any() ?? false)
                {
                    var enImage = images.movieposter.Where(x => x.lang == "en").OrderByDescending(x => x.likes).Select(x => x.url).FirstOrDefault();
                    if (enImage == null)
                    {
                        return images.movieposter.OrderByDescending(x => x.likes).Select(x => x.url).FirstOrDefault();
                    }
                    return enImage;
                }

                if (images.moviethumb?.Any() ?? false)
                {
                    return images.moviethumb.OrderBy(x => x.likes).Select(x => x.url).FirstOrDefault();
                }
            }
            else
            {
                var tv = (await _tvSearchEngineV2.Popular(0, 10, "en")).ToArray();
                var selectedMovieIndex = rand.Next(tv.Count());
                var selected = tv[selectedMovieIndex];

                return $"https://image.tmdb.org/t/p/original{selected.BackdropPath}";
            }
            return "";
        }

        [HttpGet("poster/movie/{movieDbId}")]
        public async Task<string> GetMoviePoster(string movieDbId)
        {
            var key = await _cache.GetOrAddAsync(CacheKeys.FanartTv, () => Config.GetAsync(Store.Entities.ConfigurationTypes.FanartTv), DateTimeOffset.Now.AddDays(1));

            var images = await _cache.GetOrAddAsync($"{CacheKeys.FanartTv}movie{movieDbId}", () => FanartTvApi.GetMovieImages(movieDbId, key.Value), DateTimeOffset.Now.AddDays(1));

            if (images == null)
            {
                return string.Empty;
            }

            if (images.movieposter?.Any() ?? false)
            {
                var enImage = images.movieposter.Where(x => x.lang == "en").OrderByDescending(x => x.likes).Select(x => x.url).FirstOrDefault();
                if (enImage == null)
                {
                    return images.movieposter.OrderByDescending(x => x.likes).Select(x => x.url).FirstOrDefault();
                }
                return enImage;
            }

            if (images.moviethumb?.Any() ?? false)
            {
                return images.moviethumb.OrderBy(x => x.likes).Select(x => x.url).FirstOrDefault();
            }

            return string.Empty;
        }

        [HttpGet("poster/tv/{tvdbid}")]
        public async Task<string> GetTvPoster(int tvdbid)
        {
            if (tvdbid <= 0)
            {
                return string.Empty;
            }
            var key = await _cache.GetOrAddAsync(CacheKeys.FanartTv, () => Config.GetAsync(Store.Entities.ConfigurationTypes.FanartTv), DateTimeOffset.Now.AddDays(1));

            var images = await _cache.GetOrAddAsync($"{CacheKeys.FanartTv}tv{tvdbid}", () => FanartTvApi.GetTvImages(tvdbid, key.Value), DateTimeOffset.Now.AddDays(1));

            if (images == null)
            {
                return string.Empty;
            }

            if (images.tvposter?.Any() ?? false)
            {
                var enImage = images.tvposter.Where(x => x.lang == "en").OrderByDescending(x => x.likes).Select(x => x.url).FirstOrDefault();
                if (enImage == null)
                {
                    return images.tvposter.OrderByDescending(x => x.likes).Select(x => x.url).FirstOrDefault();
                }
                return enImage;
            }

            if (images.tvthumb?.Any() ?? false)
            {
                return images.tvthumb.OrderBy(x => x.likes).Select(x => x.url).FirstOrDefault();
            }

            return string.Empty;
        }

        [HttpGet("poster/tv/tmdb/{tmdbId}")]
        public async Task<string> GetTmdbTvPoster(string tmdbId)
        {
            var images = await _cache.GetOrAddAsync($"{CacheKeys.TmdbImages}tv{tmdbId}", () => _movieDbApi.GetTvImages(tmdbId, HttpContext.RequestAborted), DateTimeOffset.Now.AddDays(1));

            if (images?.posters?.Any() ?? false)
            {
                return images.posters.Select(x => x.file_path).FirstOrDefault();
            }

            if (images?.backdrops?.Any() ?? false)
            {
                return images.backdrops.Select(x => x.file_path).FirstOrDefault();
            }
            return string.Empty;
        }

        [HttpGet("background/movie/{movieDbId}")]
        public async Task<string> GetMovieBackground(string movieDbId)
        {
            var key = await _cache.GetOrAddAsync(CacheKeys.FanartTv, () => Config.GetAsync(Store.Entities.ConfigurationTypes.FanartTv), DateTimeOffset.Now.AddDays(1));

            var images = await _cache.GetOrAddAsync($"{CacheKeys.FanartTv}movie{movieDbId}", () => FanartTvApi.GetMovieImages(movieDbId, key.Value), DateTimeOffset.Now.AddDays(1));

            if (images == null)
            {
                return string.Empty;
            }

            if (images.moviebackground?.Any() ?? false)
            {
                var enImage = images.moviebackground.Where(x => x.lang == "en").OrderByDescending(x => x.likes).Select(x => x.url).FirstOrDefault();
                if (enImage == null)
                {
                    return images.moviebackground.OrderByDescending(x => x.likes).Select(x => x.url).FirstOrDefault();
                }
                return enImage;
            }

            return string.Empty;
        }

        [HttpGet("banner/movie/{movieDbId}")]
        public async Task<string> GetMovieBanner(string movieDbId)
        {
            var key = await _cache.GetOrAddAsync(CacheKeys.FanartTv, () => Config.GetAsync(Store.Entities.ConfigurationTypes.FanartTv), DateTimeOffset.Now.AddDays(1));

            var images = await _cache.GetOrAddAsync($"{CacheKeys.FanartTv}movie{movieDbId}", () => FanartTvApi.GetMovieImages(movieDbId, key.Value), DateTimeOffset.Now.AddDays(1));

            if (images == null)
            {
                return string.Empty;
            }

            if (images.moviebackground?.Any() ?? false)
            {
                var enImage = images.moviebackground.Where(x => x.lang == "en").OrderByDescending(x => x.likes).Select(x => x.url).FirstOrDefault();
                if (enImage == null)
                {
                    return images.moviebackground.OrderByDescending(x => x.likes).Select(x => x.url).FirstOrDefault();
                }
                return enImage;
            }

            return string.Empty;
        }

        [HttpGet("background/tv/{tvdbid}")]
        public async Task<string> GetTvBackground(int tvdbid)
        {
            if (tvdbid <= 0)
            {
                return string.Empty;
            }

            return await _imageService.GetTvBackground(tvdbid.ToString());
        }

        [HttpGet("background")]
        public async Task<object> GetBackgroundImage()
        {
            var moviesArray = Options.Movies ?? new int[0];
            var tvArray = Options.TvShows ?? new int[0];

            var rand = new Random();
            var movieUrl = string.Empty;
            var tvUrl = string.Empty;

            var key = await _cache.GetOrAddAsync(CacheKeys.FanartTv, () => Config.GetAsync(Store.Entities.ConfigurationTypes.FanartTv), DateTimeOffset.Now.AddDays(1));

            if (moviesArray.Length > 0)
            {
                var item = rand.Next(moviesArray.Length);
                var result = await _cache.GetOrAddAsync($"{CacheKeys.FanartTv}movie{moviesArray[item]}", () => FanartTvApi.GetMovieImages(moviesArray[item].ToString(), key.Value), DateTimeOffset.Now.AddDays(1));

                while (!result.moviebackground?.Any() ?? true)
                {
                    item = rand.Next(moviesArray.Length);
                    result = await _cache.GetOrAddAsync($"{CacheKeys.FanartTv}movie{moviesArray[item]}", () => FanartTvApi.GetMovieImages(moviesArray[item].ToString(), key.Value), DateTimeOffset.Now.AddDays(1));

                }

                var otherRand = new Random();
                var res = otherRand.Next(result.moviebackground.Length);

                movieUrl = result.moviebackground[res].url;
            }
            if (tvArray.Length > 0)
            {
                var item = rand.Next(tvArray.Length);
                var result = await _cache.GetOrAddAsync($"{CacheKeys.FanartTv}tv{tvArray[item]}", () => FanartTvApi.GetTvImages(tvArray[item], key.Value), DateTimeOffset.Now.AddDays(1));

                while (!result.showbackground?.Any() ?? true)
                {
                    item = rand.Next(tvArray.Length);
                    result = await _cache.GetOrAddAsync($"{CacheKeys.FanartTv}tv{tvArray[item]}", () => FanartTvApi.GetTvImages(tvArray[item], key.Value), DateTimeOffset.Now.AddDays(1));
                }
                var otherRand = new Random();
                var res = otherRand.Next(result.showbackground.Length);

                tvUrl = result.showbackground[res].url;
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

        [HttpGet("background/info")]
        public async Task<object> GetBackgroundImageWithInfo()
        {
            var moviesArray = Options.Movies ?? Array.Empty<int>();
            var tvArray = Options.TvShows ?? Array.Empty<int>();

            var rand = new Random();
            var movieUrl = string.Empty;
            var movieName = string.Empty;
            var tvName = string.Empty;
            var tvUrl = string.Empty;

            var key = await _cache.GetOrAddAsync(CacheKeys.FanartTv, () => Config.GetAsync(Store.Entities.ConfigurationTypes.FanartTv), DateTimeOffset.Now.AddDays(1));

            if (moviesArray.Length > 0)
            {
                var item = rand.Next(moviesArray.Length);
                var result = await _cache.GetOrAddAsync($"{CacheKeys.FanartTv}movie{moviesArray[item]}", () => FanartTvApi.GetMovieImages(moviesArray[item].ToString(), key.Value), DateTimeOffset.Now.AddDays(1));

                while (!result.moviebackground?.Any() ?? true)
                {
                    item = rand.Next(moviesArray.Length);
                    result = await _cache.GetOrAddAsync($"{CacheKeys.FanartTv}movie{moviesArray[item]}", () => FanartTvApi.GetMovieImages(moviesArray[item].ToString(), key.Value), DateTimeOffset.Now.AddDays(1));
                }

                var otherRand = new Random();
                var res = otherRand.Next(result.moviebackground.Length);

                movieUrl = result.moviebackground[res].url;
                movieName = result.name;
            }
            if (tvArray.Length > 0)
            {
                var item = rand.Next(tvArray.Length);
                var result = await _cache.GetOrAddAsync($"{CacheKeys.FanartTv}tv{tvArray[item]}", () => FanartTvApi.GetTvImages(tvArray[item], key.Value), DateTimeOffset.Now.AddDays(1));

                while (!result.showbackground?.Any() ?? true)
                {
                    item = rand.Next(tvArray.Length);
                    result = await _cache.GetOrAddAsync($"{CacheKeys.FanartTv}tv{tvArray[item]}", () => FanartTvApi.GetTvImages(tvArray[item], key.Value), DateTimeOffset.Now.AddDays(1));
                }
                var otherRand = new Random();
                var res = otherRand.Next(result.showbackground.Length);

                tvUrl = result.showbackground[res].url;
                tvName = result.name;
            }

            if (!string.IsNullOrEmpty(movieUrl) && !string.IsNullOrEmpty(tvUrl))
            {
                var result = rand.Next(2);
                if (result == 0) return new { url = movieUrl, name = movieName };
                if (result == 1) return new { url = tvUrl, name = tvName };
            }

            if (!string.IsNullOrEmpty(movieUrl))
            {
                return new { url = movieUrl, name = movieName };
            }
            return new { url = tvUrl, name = tvName };
        }
    }
}
