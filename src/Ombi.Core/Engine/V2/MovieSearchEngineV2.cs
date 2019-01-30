using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Ombi.Core.Models.Search.V2;

namespace Ombi.Core.Engine
{
    public class MovieSearchEngineV2 : BaseMediaEngine, IMovieEngineV2
    {
        public MovieSearchEngineV2(IPrincipal identity, IRequestServiceMain service, IMovieDbApi movApi, IMapper mapper,
            ILogger<MovieSearchEngineV2> logger, IRuleEvaluator r, OmbiUserManager um, ICacheService mem, ISettingsService<OmbiSettings> s, IRepository<RequestSubscription> sub)
            : base(identity, service, r, um, mem, s, sub)
        {
            MovieApi = movApi;
            Mapper = mapper;
            Logger = logger;
        }

        private IMovieDbApi MovieApi { get; }
        private IMapper Mapper { get; }
        private ILogger Logger { get; }


        public async Task<MovieFullInfoViewModel> GetFullMovieInformation(int theMovieDbId, string langCode = null)
        {
            langCode = await DefaultLanguageCode(langCode);
            var movieInfo = await MovieApi.GetFullMovieInfo(theMovieDbId, langCode);

            return await ProcessSingleMovie(movieInfo);
        }

        private async Task<MovieFullInfoViewModel> ProcessSingleMovie(FullMovieInfo movie)
        {
            var retVal = new MovieFullInfoViewModel();

            retVal.Id = movie.Id; // TheMovieDbId
            retVal.ImdbId = movie.ImdbId;
            retVal.ReleaseDates = new ReleaseDatesDto
            {
                Results = new List<ReleaseResultsDto>()
            };

            retVal.TheMovieDbId = movie.Id.ToString();

            var viewMovie = Mapper.Map<SearchMovieViewModel>(movie);
            await RunSearchRules(viewMovie);

            // This requires the rules to be run first to populate the RequestId property
            await CheckForSubscription(viewMovie);
            var mapped = Mapper.Map<MovieFullInfoViewModel>(movie);

            mapped.Available = viewMovie.Available;
            mapped.RequestId = viewMovie.RequestId;
            mapped.Requested = viewMovie.Requested;
            mapped.PlexUrl = viewMovie.PlexUrl;
            mapped.EmbyUrl = viewMovie.EmbyUrl;
            mapped.Subscribed = viewMovie.Subscribed;
            mapped.ShowSubscribe = viewMovie.ShowSubscribe;

            return mapped;
        }

        private async Task CheckForSubscription(SearchMovieViewModel viewModel)
        {
            // Check if this user requested it
            var user = await GetUser();
            if (user == null)
            {
                return;
            }
            var request = await RequestService.MovieRequestService.GetAll()
                .AnyAsync(x => x.RequestedUserId.Equals(user.Id) && x.TheMovieDbId == viewModel.Id);
            if (request)
            {
                viewModel.ShowSubscribe = false;
            }
            else
            {
                viewModel.ShowSubscribe = true;
                var sub = await _subscriptionRepository.GetAll().FirstOrDefaultAsync(s => s.UserId == user.Id
                                                                                          && s.RequestId == viewModel.RequestId && s.RequestType == RequestType.Movie);
                viewModel.Subscribed = sub != null;
            }
        }
    }
}