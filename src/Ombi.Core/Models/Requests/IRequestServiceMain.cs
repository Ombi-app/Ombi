using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Models.Requests
{
    public interface IRequestServiceMain
    {
        IMovieRequestRepository MovieRequestService { get; }
        ITvRequestRepository TvRequestService { get; }
        IMusicRequestRepository MusicRequestRepository { get; }
    }
}