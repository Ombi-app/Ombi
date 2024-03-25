using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.Emby;
using Ombi.Api.Emby.Models;
using Ombi.Api.Emby.Models.Media.Tv;
using Ombi.Api.Emby.Models.Movie;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Schedule.Jobs.Emby
{
    public class EmbyPlayedSync : EmbyLibrarySync, IEmbyPlayedSync
    {
        public EmbyPlayedSync(
            ISettingsService<EmbySettings> settings,
            IEmbyApiFactory api,
            ILogger<EmbyContentSync> logger,
            IUserPlayedMovieRepository movieRepo,
            IUserPlayedEpisodeRepository episodeRepo,
            IEmbyContentRepository contentRepo,
            INotificationHubService notification,
            OmbiUserManager user) : base(settings, api, logger, notification)
        {
            _userManager = user;
            _movieRepo = movieRepo;
            _contentRepo = contentRepo;
            _episodeRepo = episodeRepo;
        }
        private OmbiUserManager _userManager { get; }

        private readonly IUserPlayedMovieRepository _movieRepo;
        private readonly IUserPlayedEpisodeRepository _episodeRepo;
        private readonly IEmbyContentRepository _contentRepo;

        protected async override Task ProcessTv(EmbyServers server, string parentId = default)
        {
            var allUsers = await _userManager.Users.Where(x => x.UserType == UserType.EmbyUser || x.UserType == UserType.EmbyConnectUser).ToListAsync();
            foreach (var user in allUsers)
            {
                await ProcessTvUser(server, user, parentId);
            }
        }

        protected async override Task ProcessMovies(EmbyServers server, string parentId = default)
        {

            var allUsers = await _userManager.Users.Where(x => x.UserType == UserType.EmbyUser || x.UserType == UserType.EmbyConnectUser).ToListAsync();
            foreach (var user in allUsers)
            {
                await ProcessMoviesUser(server, user, parentId);
            }
        }


        private async Task ProcessMoviesUser(EmbyServers server, OmbiUser user, string parentId = default)
        {
            EmbyItemContainer<EmbyMovie> movies;
            if (recentlyAdded)
            {
                var recentlyAddedAmountToTake = 5; // to be adjusted?
                movies = await Api.GetMoviesPlayed(server.ApiKey, parentId, 0, recentlyAddedAmountToTake, user.ProviderUserId, server.FullUri);
                // Setting this so we don't attempt to grab more than we need
                if (movies.TotalRecordCount > recentlyAddedAmountToTake)
                {
                    movies.TotalRecordCount = recentlyAddedAmountToTake;
                }
            }
            else
            {
                movies = await Api.GetMoviesPlayed(server.ApiKey, parentId, 0, AmountToTake, user.ProviderUserId, server.FullUri);
            }
            var totalCount = movies.TotalRecordCount;
            var processed = 0;
            var mediaToAdd = new HashSet<UserPlayedMovie>();

            while (processed < totalCount)
            {
                foreach (var movie in movies.Items)
                {
                    await ProcessMovie(movie, user, mediaToAdd, server);
                    processed++;
                }

                // Get the next batch
                // Recently Added should never be checked as the TotalRecords should equal the amount to take
                if (!recentlyAdded)
                {
                    movies = await Api.GetMoviesPlayed(server.ApiKey, parentId, processed, AmountToTake, user.ProviderUserId, server.FullUri);
                }
                await _movieRepo.AddRange(mediaToAdd);
                mediaToAdd.Clear();
            }
        }

        private async Task ProcessMovie(EmbyMovie movieInfo, OmbiUser user, ICollection<UserPlayedMovie> content, EmbyServers server)
        {
            if (movieInfo.ProviderIds.Tmdb.IsNullOrEmpty())
            {
                _logger.LogWarning($"Movie {movieInfo.Name} has no relevant metadata. Skipping.");
                return;
            }
            var userPlayedMovie = new UserPlayedMovie()
            {
                TheMovieDbId = int.Parse(movieInfo.ProviderIds.Tmdb),
                UserId = user.Id
            };
            // Check if it exists
            var existingMovie = await _movieRepo.Get(userPlayedMovie.TheMovieDbId, userPlayedMovie.UserId);
            var alreadyGoingToAdd = content.Any(x => x.TheMovieDbId == userPlayedMovie.TheMovieDbId && x.UserId == userPlayedMovie.UserId);
            if (existingMovie == null && !alreadyGoingToAdd)
            {
                content.Add(userPlayedMovie);
            }
        }

        private async Task ProcessTvUser(EmbyServers server, OmbiUser user, string parentId = default)
        {
            EmbyItemContainer<EmbyEpisodes> episodes;
            if (recentlyAdded)
            {
                var recentlyAddedAmountToTake = 10; // to be adjusted?
                episodes = await Api.GetTvPlayed(server.ApiKey, parentId, 0, recentlyAddedAmountToTake, user.ProviderUserId, server.FullUri);
                // Setting this so we don't attempt to grab more than we need
                if (episodes.TotalRecordCount > recentlyAddedAmountToTake)
                {
                    episodes.TotalRecordCount = recentlyAddedAmountToTake;
                }
            }
            else
            {
                episodes = await Api.GetTvPlayed(server.ApiKey, parentId, 0, AmountToTake, user.ProviderUserId, server.FullUri);
            }
            var totalCount = episodes.TotalRecordCount;
            var processed = 0;
            var mediaToAdd = new HashSet<UserPlayedEpisode>();

            while (processed < totalCount)
            {
                foreach (var episode in episodes.Items)
                {
                    await ProcessTv(episode, user, mediaToAdd, server);
                    processed++;
                }

                // Get the next batch
                // Recently Added should never be checked as the TotalRecords should equal the amount to take
                if (!recentlyAdded)
                {
                    episodes = await Api.GetTvPlayed(server.ApiKey, parentId, processed, AmountToTake, user.ProviderUserId, server.FullUri);
                }
                await _episodeRepo.AddRange(mediaToAdd);
                mediaToAdd.Clear();
            }
        }


        private async Task ProcessTv(EmbyEpisodes episode, OmbiUser user, ICollection<UserPlayedEpisode> content, EmbyServers server)
        {

            var parent = await _contentRepo.GetByEmbyId(episode.SeriesId);
            if (parent == null)
            {
                _logger.LogInformation("The episode {0} does not relate to a series, so we cannot save this",
                    episode.Name);
                return;
            }
            if (parent.TheMovieDbId.IsNullOrEmpty())
            {
                _logger.LogWarning($"Episode {episode.Name} for Tv Show {parent.Title} Doesn't have a valid TheMovieDbId. Skipping.");
                return;
            }
            if (!int.TryParse(parent.TheMovieDbId, out var parentMovieDb))
            {
                _logger.LogWarning($"Episode {episode.Name} for Tv Show {parent.Title} Doesn't have a valid TheMovieDbId. Skipping.");
                return;
            }

            await AddToContent(content, new UserPlayedEpisode()
            {
                TheMovieDbId = parentMovieDb,
                SeasonNumber = episode.ParentIndexNumber,
                EpisodeNumber = episode.IndexNumber,
                UserId = user.Id
            });

            if (episode.IndexNumberEnd.HasValue && episode.IndexNumberEnd.Value != episode.IndexNumber)
            {
                int episodeNumber = episode.IndexNumber;
                do
                {
                    _logger.LogDebug($"Multiple-episode file detected. Adding episode ${episodeNumber}");
                    episodeNumber++;

                    await AddToContent(content, new UserPlayedEpisode()
                    {
                        TheMovieDbId = parentMovieDb,
                        SeasonNumber = episode.ParentIndexNumber,
                        EpisodeNumber = episodeNumber,
                        UserId = user.Id
                    });


                } while (episodeNumber < episode.IndexNumberEnd.Value);

            }
        }

        private async Task AddToContent(ICollection<UserPlayedEpisode> content, UserPlayedEpisode episode)
        {

            // Check if it exists
            var existingEpisode = await _episodeRepo.Get(episode.TheMovieDbId, episode.SeasonNumber, episode.EpisodeNumber, episode.UserId);
            var alreadyGoingToAdd = content.Any(x =>
                x.TheMovieDbId == episode.TheMovieDbId
                && x.SeasonNumber == episode.SeasonNumber
                && x.EpisodeNumber == episode.EpisodeNumber
                && x.UserId == episode.UserId);
            if (existingEpisode == null && !alreadyGoingToAdd)
            {
                content.Add(episode);
            }
        }
    }


}
