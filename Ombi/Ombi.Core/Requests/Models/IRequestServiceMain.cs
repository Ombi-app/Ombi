using Ombi.Core.Requests.Models;

namespace Ombi.Core.Models.Requests
{
    public interface IRequestServiceMain
    {
        IRequestService<MovieRequestModel> MovieRequestService { get; }
        IRequestService<TvRequestModel> TvRequestService { get; }
    }
}