using System.Threading.Tasks;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core
{
    public interface IMovieSender
    {
        Task<MovieSenderResult> Send(MovieRequests model, string qualityId = "");
    }
}