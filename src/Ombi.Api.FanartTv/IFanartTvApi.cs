using System.Threading.Tasks;
using Ombi.Api.FanartTv.Models;

namespace Ombi.Api.FanartTv
{
    public interface IFanartTvApi
    {
        Task<MovieResult> GetMovieImages(int theMovieDbId, string token);
        Task<TvResult> GetTvImages(int tvdbId, string token);
    }
}