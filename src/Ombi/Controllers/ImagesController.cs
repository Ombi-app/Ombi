using Microsoft.AspNetCore.Mvc;
using Ombi.Api.FanartTv;
using Ombi.Store.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Ombi.Config;

namespace Ombi.Controllers
{
    [ApiV1]
    [Produces("application/json")]
    public class ImagesController : Controller
    {
        public ImagesController(IFanartTvApi api, IApplicationConfigRepository config, IOptions<LandingPageBackground> options)
        {
            Api = api;
            Config = config;
            Options = options.Value;
        }

        private IFanartTvApi Api { get; }
        private IApplicationConfigRepository Config { get; }
        private LandingPageBackground Options { get; }

        [HttpGet("background")]
        public async Task<object> GetBackgroundImage()
        {
            var moviesArray = Options.Movies;
            var tvArray = Options.TvShows;

            var rand = new Random();
            var movieUrl = string.Empty;
            var tvUrl = string.Empty;

            if (moviesArray.Any())
            {
                var item = rand.Next(moviesArray.Length);
                var key = await Config.Get(Store.Entities.ConfigurationTypes.FanartTv);
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
                var key = await Config.Get(Store.Entities.ConfigurationTypes.FanartTv);
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
