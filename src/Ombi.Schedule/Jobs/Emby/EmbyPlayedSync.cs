using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.Emby;
using Ombi.Api.Emby.Models;
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
        public EmbyPlayedSync(ISettingsService<EmbySettings> settings, IEmbyApiFactory api, ILogger<EmbyContentSync> logger,
            IUserPlayedMovieRepository repo, INotificationHubService notification, OmbiUserManager user) : base(settings, api, logger, notification)
        {
            _userManager = user;
            _repo = repo;
        }
        private OmbiUserManager _userManager { get; }

        private readonly IUserPlayedMovieRepository _repo;

        protected override Task ProcessTv(EmbyServers server, string parentId = default)
        {
            // TODO
            return Task.CompletedTask;
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
                await _repo.AddRange(mediaToAdd);
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
            var existingMovie = await _repo.Get(userPlayedMovie.TheMovieDbId, userPlayedMovie.UserId);
            var alreadyGoingToAdd = content.Any(x => x.TheMovieDbId == userPlayedMovie.TheMovieDbId && x.UserId == userPlayedMovie.UserId);
            if (existingMovie == null && !alreadyGoingToAdd)
            {
                content.Add(userPlayedMovie);
            }
        }
    }

}
