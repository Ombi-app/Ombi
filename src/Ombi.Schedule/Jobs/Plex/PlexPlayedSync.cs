using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Schedule.Jobs.Plex
{
    public class PlexPlayedSync : PlexLibrarySync, IPlexPlayedSync
    {
        public PlexPlayedSync(
            ISettingsService<PlexSettings> plex, 
            IPlexApi plexApi, 
            ILogger<PlexLibrarySync> logger, 
            IPlexContentRepository contentRepo,
            INotificationHubService notificationHubService,
            OmbiUserManager user,
            IUserPlayedMovieRepository movieRepo,
            IUserPlayedEpisodeRepository episodeRepo):
            base(plex, plexApi, logger, notificationHubService)
        {
            _contentRepo = contentRepo;
            _userManager = user;
            _movieRepo = movieRepo;
            _episodeRepo = episodeRepo;
            Plex.ClearCache();
        }

        private IPlexContentRepository _contentRepo { get; }
        private OmbiUserManager _userManager { get; }
        private readonly IUserPlayedMovieRepository _movieRepo;
        private readonly IUserPlayedEpisodeRepository _episodeRepo;
        
        private const int recentlyAddedAmountToTake = 5;

        protected override async Task ProcessServer(PlexServers servers)
        {
            var allUsers = await _userManager.GetPlexUsersWithValidTokens();
           foreach (var user in allUsers)
            {
                await ProcessUser(servers, recentlyAddedSearch, user);
            }
        }

        private async Task ProcessUser(PlexServers servers, bool recentlyAddedSearch, OmbiUser user)
        {
            
            var contentProcessed = new Dictionary<int, string>();
            var episodesProcessed = new List<int>();
            Logger.LogDebug($"Getting all played content from server {servers.Name} for user {user.Alias}");
            var allContent = await GetAllContent(servers, recentlyAddedSearch, user);
            Logger.LogDebug("We found {0} items", allContent.Count);


            // Let's now process this.
            var episodesToAdd = new HashSet<UserPlayedEpisode>();
            var moviesToAdd = new HashSet<UserPlayedMovie>();


            foreach (var content in allContent.OrderByDescending(x => x.viewGroup))
            {
                Logger.LogDebug($"Got type '{content.viewGroup}' to process");
                if (content.viewGroup.Equals(PlexMediaType.Show.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (var epInfo in content.Metadata ?? new Metadata[] { })
                    {
                        await ProcessEpisode(epInfo, user, episodesToAdd);
                    }

                }
                if (content.viewGroup.Equals(PlexMediaType.Movie.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    Logger.LogDebug("Processing Movies");
                    foreach (var movie in content?.Metadata ?? Array.Empty<Metadata>())
                    {
                        await ProcessMovie(movie, user, moviesToAdd);
                    }
                }
            }

            await _movieRepo.AddRange(moviesToAdd);
            await _episodeRepo.AddRange(episodesToAdd);

        }

        private async Task ProcessEpisode(Metadata epInfo, OmbiUser user, ICollection<UserPlayedEpisode> content)
        {
            var episode = await _contentRepo.GetEpisodeByKey(epInfo.ratingKey);
            if (episode == null || episode.Series == null)
            {
                Logger.LogInformation($"The episode {epInfo.title} does not relate to a series, so we cannot save this");
                return;
            }
            if (episode.Series.TheMovieDbId.IsNullOrEmpty())
            {
                Logger.LogWarning($"Episode {epInfo.title} is not linked to a TMDB series. Skipping.");
                return;
            }

            await AddToContent(content, new UserPlayedEpisode()
            {
                TheMovieDbId = int.Parse(episode.Series.TheMovieDbId),
                SeasonNumber = episode.SeasonNumber,
                EpisodeNumber = episode.EpisodeNumber,
                UserId = user.Id
            });

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

        public async Task ProcessMovie(Metadata movie, OmbiUser user, ICollection<UserPlayedMovie> content)
        {
            var cachedMovie = await _contentRepo.GetByKey(movie.ratingKey);
            if (cachedMovie == null  || cachedMovie.TheMovieDbId.IsNullOrEmpty() )
            {
                Logger.LogWarning($"Movie {movie.title} has no relevant metadata. Skipping.");
                return;
            }
            var userPlayedMovie = new UserPlayedMovie()
            {
                TheMovieDbId = int.Parse(cachedMovie.TheMovieDbId),
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


        private async Task<List<Mediacontainer>> GetAllContent(PlexServers plexSettings, bool recentlyAddedSearch, OmbiUser user)
        {
            var libs = new List<Mediacontainer>();

            var directories = await GetEnabledLibraries(plexSettings);

            foreach (var directory in directories)
            {
                var maxNumberOfItems = 0;
                if (recentlyAddedSearch)
                {
                    maxNumberOfItems = recentlyAddedAmountToTake;
                }
                var container = await PlexApi.GetPlayed(user.MediaServerToken, plexSettings.FullUri,
                    directory.key, maxNumberOfItems);
                if (container != null) 
                {
                    libs.Add(container.MediaContainer);
                }
            }

            return libs;
        }


        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}