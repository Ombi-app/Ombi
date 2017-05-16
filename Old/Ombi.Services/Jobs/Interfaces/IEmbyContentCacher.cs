using System.Collections.Generic;
using Ombi.Api.Models.Emby;
using Quartz;

namespace Ombi.Services.Jobs.Interfaces
{
    public interface IEmbyContentCacher
    {
        void CacheContent();
        void Execute(IJobExecutionContext context);
        List<EmbyMovieItem> GetMovies();
        List<EmbySeriesItem> GetTvShows();
    }
}