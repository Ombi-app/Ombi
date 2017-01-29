#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: PlexAvailabilityChecker.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NLog;
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Plex;
using Ombi.Core;
using Ombi.Core.Models;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Services.Interfaces;
using Ombi.Services.Models;
using Ombi.Store;
using Ombi.Store.Models;
using Ombi.Store.Models.Emby;
using Ombi.Store.Models.Plex;
using Ombi.Store.Repository;
using Quartz;
using PlexMediaType = Ombi.Api.Models.Plex.PlexMediaType;

namespace Ombi.Services.Jobs
{
    public class EmbyAvailabilityChecker : IJob, IEmbyAvailabilityChecker
    {
        public EmbyAvailabilityChecker(ISettingsService<EmbySettings> embySettings, IRequestService request, IEmbyApi emby, ICacheProvider cache,
            INotificationService notify, IJobRecord rec, IRepository<UsersToNotify> users, IRepository<EmbyEpisodes> repo, INotificationEngine e, IRepository<EmbyContent> content)
        {
            Emby = embySettings;
            RequestService = request;
            EmbyApi = emby;
            Cache = cache;
            Notification = notify;
            Job = rec;
            UserNotifyRepo = users;
            EpisodeRepo = repo;
            NotificationEngine = e;
            EmbyContent = content;
        }

        private ISettingsService<EmbySettings> Emby { get; }
        private IRepository<EmbyEpisodes> EpisodeRepo { get; }
        private IRequestService RequestService { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private IEmbyApi EmbyApi { get; }
        private ICacheProvider Cache { get; }
        private INotificationService Notification { get; }
        private IJobRecord Job { get; }
        private IRepository<UsersToNotify> UserNotifyRepo { get; }
        private INotificationEngine NotificationEngine { get; }
        private IRepository<EmbyContent> EmbyContent { get; }

        public void CheckAndUpdateAll()
        {
            var embySettings = Emby.GetSettings();

            if (!ValidateSettings(embySettings))
            {
                Log.Debug("Validation of the Emby settings failed.");
                return;
            }

            var content = EmbyContent.GetAll().ToList();

            var movies = GetEmbyMovies(content).ToArray();
            var shows = GetEmbyTvShows(content).ToArray();
            var albums = GetEmbyMusic(content).ToArray();

            var requests = RequestService.GetAll();
            var requestedModels = requests as RequestedModel[] ?? requests.Where(x => !x.Available).ToArray();

            if (!requestedModels.Any())
            {
                Log.Debug("There are no requests to check.");
                return;
            }

            var modifiedModel = new List<RequestedModel>();
            foreach (var r in requestedModels)
            {
                var releaseDate = r.ReleaseDate == DateTime.MinValue ? string.Empty : r.ReleaseDate.ToString("yyyy");
                bool matchResult;

                switch (r.Type)
                {
                    case RequestType.Movie:
                        matchResult = IsMovieAvailable(movies, r.Title, releaseDate, r.ImdbId);
                        break;
                    case RequestType.TvShow:
                        if (!embySettings.EnableEpisodeSearching)
                        {
                            matchResult = IsTvShowAvailable(shows, r.Title, releaseDate, r.TvDbId, r.SeasonList);
                        }
                        else
                        {
                            matchResult = r.Episodes.Any() ?
                                r.Episodes.All(x => IsEpisodeAvailable(r.TvDbId, x.SeasonNumber, x.EpisodeNumber)) :
                                IsTvShowAvailable(shows, r.Title, releaseDate, r.TvDbId, r.SeasonList);
                        }
                        break;
                    case RequestType.Album:
                        //matchResult = IsAlbumAvailable(albums, r.Title, r.ReleaseDate.Year.ToString(), r.ArtistName); // TODO Emby
                        matchResult = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


                if (matchResult)
                {
                    r.Available = true;
                    modifiedModel.Add(r);
                    continue;
                }

            }

            Log.Debug("Requests that will be updated count {0}", modifiedModel.Count);

            if (modifiedModel.Any())
            {
                //NotificationEngine.NotifyUsers(modifiedModel, embySettings.ApiKey, NotificationType.RequestAvailable); // TODO Emby
                RequestService.BatchUpdate(modifiedModel);
            }
        }

        public IEnumerable<EmbyContent> GetEmbyMovies(IEnumerable<EmbyContent> content)
        {
            return content.Where(x => x.Type == EmbyMediaType.Movie);
        }

        public bool IsMovieAvailable(EmbyContent[] embyMovies, string title, string year, string providerId)
        {
            var movie = GetMovie(embyMovies, title, year, providerId);
            return movie != null;
        }

        public EmbyContent GetMovie(EmbyContent[] embyMovies, string title, string year, string providerId)
        {
            if (embyMovies.Length == 0)
            {
                return null;
            }
            foreach (var movie in embyMovies)
            {
                if (string.IsNullOrEmpty(movie.Title) || movie.PremierDate == DateTime.MinValue)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(movie.ProviderId) &&
                    movie.ProviderId.Equals(providerId, StringComparison.InvariantCultureIgnoreCase))
                {
                    return movie;
                }

                if (movie.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase) &&
                    movie.PremierDate.Year.ToString().Equals(year, StringComparison.CurrentCultureIgnoreCase))
                {
                    return movie;
                }
            }
            return null;
        }

        public IEnumerable<EmbyContent> GetEmbyTvShows(IEnumerable<EmbyContent> content)
        {
            return content.Where(x => x.Type == EmbyMediaType.Series);
        }

        public bool IsTvShowAvailable(EmbyContent[] embyShows, string title, string year, string providerId, int[] seasons = null)
        {
            var show = GetTvShow(embyShows, title, year, providerId, seasons);
            return show != null;
        }


        public EmbyContent GetTvShow(EmbyContent[] embyShows, string title, string year, string providerId,
            int[] seasons = null)
        {
            foreach (var show in embyShows)
            {
                //if (show.ProviderId == providerId && seasons != null) // TODO Emby
                //{
                //    var showSeasons = ByteConverterHelper.ReturnObject<int[]>(show.Seasons);
                //    if (seasons.Any(season => showSeasons.Contains(season)))
                //    {
                //        return show;
                //    }
                //    return null;
                //}
                if (!string.IsNullOrEmpty(show.ProviderId) &&
                    show.ProviderId.Equals(providerId, StringComparison.InvariantCultureIgnoreCase))
                {
                    return show;
                }

                if (show.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase) &&
                    show.PremierDate.Year.ToString().Equals(year, StringComparison.CurrentCultureIgnoreCase))
                {
                    return show;
                }
            }
            return null;
        }

        public bool IsEpisodeAvailable(string theTvDbId, int season, int episode)
        {
            var ep = EpisodeRepo.Custom(
                connection =>
                {
                    connection.Open();
                    var result = connection.Query<EmbyEpisodes>("select * from EmbyEpisodes where ProviderId = @ProviderId", new { ProviderId = theTvDbId });

                    return result;
                }).ToList();

            if (!ep.Any())
            {
                Log.Info("Episode cache info is not available. tvdbid: {0}, season: {1}, episode: {2}", theTvDbId, season, episode);
                return false;
            }
            foreach (var result in ep)
            {
                if (result.ProviderId.Equals(theTvDbId) && result.EpisodeNumber == episode && result.SeasonNumber == season)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the episode's db in the cache.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<EmbyEpisodes>> GetEpisodes()
        {
            var episodes = await EpisodeRepo.GetAllAsync();
            if (episodes == null)
            {
                return new HashSet<EmbyEpisodes>();
            }
            return episodes;
        }

        /// <summary>
        /// Gets the episode's stored in the db and then filters on the TheTvDBId.
        /// </summary>
        /// <param name="theTvDbId">The tv database identifier.</param>
        /// <returns></returns>
        public async Task<IEnumerable<EmbyEpisodes>> GetEpisodes(int theTvDbId)
        {
            var ep = await EpisodeRepo.CustomAsync(async connection =>
               {
                   connection.Open();
                   var result = await connection.QueryAsync<EmbyEpisodes>("select * from EmbyEpisodes where ProviderId = @ProviderId", new { ProviderId = theTvDbId });

                   return result;
               });

            var embyEpisodes = ep as EmbyEpisodes[] ?? ep.ToArray();
            if (!embyEpisodes.Any())
            {
                Log.Info("Episode db info is not available.");
                return new List<EmbyEpisodes>();
            }

            return embyEpisodes;
        }

        public IEnumerable<EmbyContent> GetEmbyMusic(IEnumerable<EmbyContent> content)
        {
            return content.Where(x => x.Type == EmbyMediaType.Music);
        }


        private bool ValidateSettings(EmbySettings emby)
        {
            if (emby.Enable)
            {
                if (string.IsNullOrEmpty(emby?.Ip) || string.IsNullOrEmpty(emby?.ApiKey) || string.IsNullOrEmpty(emby?.AdministratorId))
                {
                    Log.Warn("A setting is null, Ensure Emby is configured correctly");
                    return false;
                }
            }
            return emby.Enable;
        }

        public void Execute(IJobExecutionContext context)
        {

            Job.SetRunning(true, JobNames.EmbyChecker);
            try
            {
                CheckAndUpdateAll();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                Job.Record(JobNames.EmbyChecker);
                Job.SetRunning(false, JobNames.EmbyChecker);
            }
        }

        public void Start()
        {
            Job.SetRunning(true, JobNames.EmbyChecker);
            try
            {
                CheckAndUpdateAll();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                Job.Record(JobNames.EmbyChecker);
                Job.SetRunning(false, JobNames.EmbyChecker);
            }
        }
    }
}