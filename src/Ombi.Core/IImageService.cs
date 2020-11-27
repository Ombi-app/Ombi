using System.Threading.Tasks;

namespace Ombi.Core
{
    public interface IImageService
    {
        Task<string> GetTvBackground(string tvdbId);
    }
}