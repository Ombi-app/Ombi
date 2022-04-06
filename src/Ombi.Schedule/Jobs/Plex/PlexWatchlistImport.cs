using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Repository.Requests;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs.Plex
{
    public class PlexWatchlistImport : IPlexWatchlistImport
    {
        private readonly IPlexApi _plexApi;
        private readonly ISettingsService<PlexSettings> _settings;
        private readonly OmbiUserManager _ombiUserManager;
        private readonly IMovieRequestRepository _movieRequestRepository;
        private readonly ITvRequestRepository _tvRequestRepository;
        private readonly IMovieRequestEngine _movieRequestEngine;

        public PlexWatchlistImport(IPlexApi plexApi, ISettingsService<PlexSettings> settings, OmbiUserManager ombiUserManager,
            IMovieRequestRepository movieRequestRepository, ITvRequestRepository tvRequestRepository, IMovieRequestEngine movieRequestEngine)
        {
            _plexApi = plexApi;
            _settings = settings;
            _ombiUserManager = ombiUserManager;
            _movieRequestRepository = movieRequestRepository;
            _tvRequestRepository = tvRequestRepository;
            _movieRequestEngine = movieRequestEngine;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var token = "-DpQi6mzq2QMakYgFr2g"; // !!!!!!!!!!!!!!!!!!!! TODO REMOVE !!!!!!!!!!!!!!!!!!!!!!!

            var settings = await _settings.GetSettingsAsync();
            if (!settings.Enable)
            {
                return;
            }

            var plexUsersWithTokens = _ombiUserManager.Users.Where(x => x.UserType == UserType.PlexUser && x.MediaServerToken != null).ToList();
            foreach (var user in plexUsersWithTokens)
            {
                var watchlist = await _plexApi.GetWatchlist(user.MediaServerToken, context.CancellationToken);
                if (watchlist == null || !watchlist.Metadata.Any())
                {
                    continue;
                }

                var items = watchlist.Metadata;
                foreach (var item in items)
                {
                    switch (item.type)
                    {
                        case "show":
                            await ProcessShow(item);
                            break;
                        case "movie":
                            await ProcessMovie(item, null);
                            break;
                    }
                }


            }
        }

        private async Task ProcessMovie(Metadata movie, PlexServers servers)
        {
            var providerIds = await GetProviderIds(movie, servers);
            if (!providerIds.TheMovieDb.HasValue())
            {
                // We need a MovieDbId to support this;
                return;
            }
            //_movieRequestEngine.RequestMovie(new() { TheMovieDbId =  });
        }

        private async Task<ProviderId> GetProviderIds(Metadata movie, PlexServers servers)
        {
            var guids = new List<string>();
            if (!movie.Guid.Any())
            {
                var metaData = await _plexApi.GetMetadata(servers.PlexAuthToken, servers.FullUri,
                    movie.ratingKey);

                var meta = metaData.MediaContainer.Metadata.FirstOrDefault();
                guids.Add(meta.guid);
                if (meta.Guid != null)
                {
                    foreach (var g in meta.Guid)
                    {
                        guids.Add(g.Id);
                    }
                }
            }
            else
            {
                // Currently a Plex Pass feature only
                foreach (var g in movie.Guid)
                {
                    guids.Add(g.Id);
                }
            }
            var providerIds = PlexHelper.GetProviderIdsFromMetadata(guids.ToArray());
            return providerIds;
        }

        private async Task ProcessShow(Metadata metadata)
        {

        }

        public void Dispose() { }
    }
}
