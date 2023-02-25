using System.Threading;
using System.Threading.Tasks;

namespace Ombi.Core
{
    public interface IImageService
    {
        Task<string> GetTvBackground(string tvdbId);
        Task<string> GetTmdbTvBackground(string id, CancellationToken token);
        Task<string> GetTmdbTvPoster(string tmdbId, CancellationToken token);
    }
}