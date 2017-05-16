using Ombi.Core.Requests.Models;

namespace Ombi.Core.Models.Requests
{
    public class RequestService : IRequestServiceMain
    {
        public RequestService(IRequestService<TvRequestModel> tv, IRequestService<MovieRequestModel> movie)
        {
            TvRequestService = tv;
            MovieRequestService = movie;
        }

        public IRequestService<TvRequestModel> TvRequestService { get; }
        public IRequestService<MovieRequestModel> MovieRequestService { get; }
    }
}