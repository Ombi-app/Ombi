using System.Threading.Tasks;
using Ombi.Api.External.ExternalApis.FanartTv.Models;

namespace Ombi.Api.External.ExternalApis.FanartTv
{
    public interface IFanartTvApi
    {
        Task<MovieResult> GetMovieImages(string movieOrImdbId, string token);
        Task<TvResult> GetTvImages(int tvdbId, string token);
    }
}