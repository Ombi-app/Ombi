using System.Linq;

using Nancy;
using Nancy.Responses.Negotiation;

using RequestPlex.Api;
using RequestPlex.Core;
using RequestPlex.Core.SettingModels;
using RequestPlex.Store;

namespace RequestPlex.UI.Modules
{
    public class RequestsModule : NancyModule
    {
        private IRepository<RequestedModel> Service { get; set; }
        public RequestsModule(IRepository<RequestedModel> service)
        {
            Service = service;

            Get["requests/"] = _ => LoadRequests();
            Get["requests/movies"] = _ => GetMovies();
            Get["requests/tvshows"] = _ => GetTvShows();
        }


        private Negotiator LoadRequests()
        {
            return View["Requests/Index"];
        }

        private Response GetMovies()
        {
            var dbMovies = Service.GetAll().Where(x => x.Type == RequestType.Movie);
            return Response.AsJson(dbMovies);
        }

        private Response GetTvShows()
        {
            var dbTv = Service.GetAll().Where(x => x.Type == RequestType.TvShow);
            return Response.AsJson(dbTv);
        }
    }
}