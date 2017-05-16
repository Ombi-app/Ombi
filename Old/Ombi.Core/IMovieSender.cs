using System.Threading.Tasks;
using Ombi.Store;

namespace Ombi.Core
{
    public interface IMovieSender
    {
        Task<MovieSenderResult> Send(RequestedModel model, string qualityId = "");
    }
}