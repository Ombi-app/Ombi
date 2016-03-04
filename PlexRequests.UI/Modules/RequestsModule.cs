#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: RequestsModule.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion
using System.Linq;

using Humanizer;

using Nancy;
using Nancy.Responses.Negotiation;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Store;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class RequestsModule : BaseModule
    {

        public RequestsModule(IRepository<RequestedModel> service, ISettingsService<PlexRequestSettings> prSettings) : base("requests")
        {
            Service = service;
            PrSettings = prSettings;

            Get["/"] = _ => LoadRequests();
            Get["/movies"] = _ => GetMovies();
            Get["/tvshows"] = _ => GetTvShows();
            Post["/delete"] = _ =>
            {
                var convertedType = (string)Request.Form.type == "movie" ? RequestType.Movie : RequestType.TvShow;
                return DeleteRequest((int)Request.Form.id, convertedType);
            };
        }
        private IRepository<RequestedModel> Service { get; }
        private ISettingsService<PlexRequestSettings> PrSettings { get; }

        private Negotiator LoadRequests()
        {
            var settings = PrSettings.GetSettings();
            return View["Requests/Index", settings];
        }

        private Response GetMovies()
        {
            var dbMovies = Service.GetAll().Where(x => x.Type == RequestType.Movie);
            var viewModel = dbMovies.Select(tv => new RequestViewModel
            {
                ProviderId = tv.ProviderId,
                Type = tv.Type,
                Status = tv.Status,
                ImdbId = tv.ImdbId,
                Id = tv.Id,
                PosterPath = tv.PosterPath,
                ReleaseDate = tv.ReleaseDate.Humanize(),
                RequestedDate = tv.RequestedDate.Humanize(),
                Approved = tv.Approved,
                Title = tv.Title,
                Overview = tv.Overview,
                RequestedBy = tv.RequestedBy,
                ReleaseYear = tv.ReleaseDate.Year.ToString()
            }).ToList();
            //TODO check if Available in CP
            return Response.AsJson(viewModel);
        }

        private Response GetTvShows()
        {
            var dbTv = Service.GetAll().Where(x => x.Type == RequestType.TvShow);
            var viewModel = dbTv.Select(tv => new RequestViewModel
            {
                ProviderId = tv.ProviderId,
                Type = tv.Type,
                Status = tv.Status,
                ImdbId = tv.ImdbId,
                Id = tv.Id,
                PosterPath = tv.ProviderId.ToString(),
                ReleaseDate = tv.ReleaseDate.Humanize(),
                RequestedDate = tv.RequestedDate.Humanize(),
                Approved = tv.Approved,
                Title = tv.Title,
                Overview = tv.Overview,
                RequestedBy = tv.RequestedBy,
                ReleaseYear = tv.ReleaseDate.Year.ToString()
            }).ToList();
            //TODO check if Available in Sonarr
            return Response.AsJson(viewModel);
        }

        private Response DeleteRequest(int providerId, RequestType type)
        {
            var currentEntity = Service.GetAll().FirstOrDefault(x => x.ProviderId == providerId && x.Type == type);
            Service.Delete(currentEntity);
            return Response.AsJson(new { Result = true });
        }
    }
}