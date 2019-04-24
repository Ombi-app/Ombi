using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Models.Requests;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Engine.V2
{
    public class MultiSearchEngine : BaseMediaEngine, IMultiSearchEngine
    {
        public MultiSearchEngine(IPrincipal identity, IRequestServiceMain requestService, IRuleEvaluator rules,
            OmbiUserManager um, ICacheService cache, ISettingsService<OmbiSettings> ombiSettings, IRepository<RequestSubscription> sub,
            IMovieDbApi movieDbApi) 
            : base(identity, requestService, rules, um, cache, ombiSettings, sub)
        {
            _movieDbApi = movieDbApi;
        }

        private readonly IMovieDbApi _movieDbApi;


        public async Task<List<MultiSearch>> MultiSearch(string searchTerm, string lang = "en")
        {
            return (await _movieDbApi.MultiSearch(searchTerm, lang)).results;
        }
    }
}
