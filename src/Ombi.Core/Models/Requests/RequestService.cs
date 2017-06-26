using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Models.Requests
{
    public class RequestService : IRequestServiceMain
    {
        public RequestService(ITvRequestRepository tv, IMovieRequestRepository movie)
        {
            TvRequestService = tv;
            MovieRequestService = movie;
        }

        public ITvRequestRepository TvRequestService { get; }
        public IMovieRequestRepository MovieRequestService { get; }
    }
}