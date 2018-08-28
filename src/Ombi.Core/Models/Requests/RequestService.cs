using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Models.Requests
{
    public class RequestService : IRequestServiceMain
    {
        public RequestService(ITvRequestRepository tv, IMovieRequestRepository movie, IMusicRequestRepository music)
        {
            TvRequestService = tv;
            MovieRequestService = movie;
            MusicRequestRepository = music;
        }

        public ITvRequestRepository TvRequestService { get; }
        public IMusicRequestRepository MusicRequestRepository { get; }
        public IMovieRequestRepository MovieRequestService { get; }
    }
}