using AutoMapper;
using Microsoft.Extensions.Options;
using Ombi.Api.Trakt;
using Ombi.Api.TvMaze;
using Ombi.Config;
using Ombi.Core.Authentication;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Ombi.Core.Engine.Demo
{
    public class DemoTvSearchEngine : TvSearchEngine, IDemoTvSearchEngine
    {

        public DemoTvSearchEngine(IPrincipal identity, IRequestServiceMain service, ITvMazeApi tvMaze, IMapper mapper,
             ITraktApi trakt, IRuleEvaluator r, OmbiUserManager um, ICacheService memCache,
            ISettingsService<OmbiSettings> s, IRepository<RequestSubscription> sub, IOptions<DemoLists> lists, IImageService imageService,
            ISettingsService<CustomizationSettings> custom)
            : base(identity, service, tvMaze, mapper, trakt, r, um, custom, memCache, s, sub, imageService)
        {
            _demoLists = lists.Value;
        }

        private readonly DemoLists _demoLists;

        new public async Task<IEnumerable<SearchTvShowViewModel>> Search(string search)
        {
            var searchResult = await TvMazeApi.Search(search);

            for (var i = 0; i < searchResult.Count; i++)
            {
                if (!_demoLists.TvShows.Contains(searchResult[i].show?.externals?.thetvdb ?? 0))
                {
                    searchResult.RemoveAt(i);
                }
            }

            if (searchResult != null)
            {
                var retVal = new List<SearchTvShowViewModel>();
                foreach (var tvMazeSearch in searchResult)
                {
                    if (tvMazeSearch.show.externals == null || !(tvMazeSearch.show.externals?.thetvdb.HasValue ?? false))
                    {
                        continue;
                    }
                    retVal.Add(await ProcessResult(tvMazeSearch, false));
                }
                return retVal;
            }
            return null;
        }

        public async Task<IEnumerable<SearchTvShowViewModel>> NowPlayingMovies()
        {
            var rand = new Random();
            var responses = new List<SearchTvShowViewModel>();
            for (int i = 0; i < 10; i++)
            {
                var item = rand.Next(_demoLists.TvShows.Length);
                var tv = _demoLists.TvShows[item];
                if (responses.Any(x => x.Id == tv))
                {
                    i--;
                    continue;
                }

                var movieResult = await TvMazeApi.ShowLookup(tv);
                responses.Add(await ProcessResult(movieResult, false));
            }

            return responses;
        }



    }

    public interface IDemoTvSearchEngine
    {
        Task<IEnumerable<SearchTvShowViewModel>> Search(string search);
        Task<IEnumerable<SearchTvShowViewModel>> NowPlayingMovies();
    }
}
