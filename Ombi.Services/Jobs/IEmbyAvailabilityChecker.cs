using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Store.Models.Emby;
using Quartz;

namespace Ombi.Services.Jobs
{
    public interface IEmbyAvailabilityChecker
    {
        void CheckAndUpdateAll();
        void Execute(IJobExecutionContext context);
        IEnumerable<EmbyContent> GetEmbyMovies(IEnumerable<EmbyContent> content);
        IEnumerable<EmbyContent> GetEmbyMusic(IEnumerable<EmbyContent> content);
        IEnumerable<EmbyContent> GetEmbyTvShows(IEnumerable<EmbyContent> content);
        Task<IEnumerable<EmbyEpisodes>> GetEpisodes();
        Task<IEnumerable<EmbyEpisodes>> GetEpisodes(int theTvDbId);
        EmbyContent GetMovie(EmbyContent[] embyMovies, string title, string year, string providerId);
        EmbyContent GetTvShow(EmbyContent[] embyShows, string title, string year, string providerId, int[] seasons = null);
        bool IsEpisodeAvailable(string theTvDbId, int season, int episode);
        bool IsMovieAvailable(EmbyContent[] embyMovies, string title, string year, string providerId);
        bool IsTvShowAvailable(EmbyContent[] embyShows, string title, string year, string providerId, int[] seasons = null);
        void Start();
    }
}