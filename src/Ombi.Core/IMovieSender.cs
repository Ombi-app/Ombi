using System.Threading.Tasks;
using Ombi.Core.Models.Requests.Movie;

namespace Ombi.Core
{
    public interface IMovieSender
    {
        Task<MovieSenderResult> Send(MovieRequestModel model, string qualityId = "");
    }
}