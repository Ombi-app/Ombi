using Microsoft.AspNetCore.Mvc;
using Ombi.Api.FanartTv;
using Ombi.Store.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ombi.Controllers
{
    [ApiV1]
    public class ImagesController : Controller
    {
        public ImagesController(IFanartTvApi api, IApplicationConfigRepository config)
        {
            Api = api;
            Config = config;
        }

        private IFanartTvApi Api { get; }
        private IApplicationConfigRepository Config { get; }

        [HttpGet("background")]
        public async Task<object> GetBackgroundImage()
        {
            var moviesArray = new[]{
                278,
                238,
                431483,
                372058,
                244786,
                680,
                155,
                13,
                1891,
                399106
            };

            var key = await Config.Get(Store.Entities.ConfigurationTypes.FanartTv);

            var result = await Api.GetMovieImages(155, key.Value);

            return new { url = result.moviebackground[0].url };

        }
    }
}
