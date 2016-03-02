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
        public RequestsModule(IRepository<RequestedModel> service) : base("requests")
        {
            Service = service;

            Get["/"] = _ => LoadRequests();
            Get["/movies"] = _ => GetMovies();
            Get["/tvshows"] = _ => GetTvShows();
            Post["/delete"] = _ =>
            {
                var convertedType = (string)Request.Form.type == "movie" ? RequestType.Movie : RequestType.TvShow;
                return Delete((int)Request.Form.id, convertedType);
            };
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

        private Response Delete(int tmdbId, RequestType type)
        {
            var currentEntity = Service.GetAll().FirstOrDefault(x => x.Tmdbid == tmdbId && x.Type == type);
            Service.Delete(currentEntity);
            return Response.AsJson(new { Result = true });
        }
    }
}