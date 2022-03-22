using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Ombi.Hubs;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Quartz;
using Ombi.Api.MediaServer.Models.Media.Tv;

namespace Ombi.Schedule.Jobs.MediaServer
{
    public abstract class MediaServerEpisodeSync<T, U, V, W> : IMediaServerEpisodeSync<U, T>
    where T : IMediaServerEpisodes
    where U : IMediaServerEpisode
    where V : IMediaServerContentRepository<W>
    where W : IMediaServerContent
    {
        public MediaServerEpisodeSync(
            ILogger l,
            V repo,
            IHubContext<NotificationHub> notification)
        {
            _logger = l;
            _repo = repo;
            _notification = notification;
        }

        protected readonly IHubContext<NotificationHub> _notification;
        protected readonly ILogger _logger;
        protected readonly V _repo;
        protected abstract IAsyncEnumerable<T> GetMediaServerEpisodes();
        protected abstract Task<U> GetExistingEpisode(T ep);
        protected abstract bool IsIn(T ep, ICollection<U> list);

        protected async Task CacheEpisodes()
        {
            var epToAdd = new HashSet<U>();
            await ProcessEpisodes(GetMediaServerEpisodes(), epToAdd);
        }

        protected abstract void addEpisode(T ep, ICollection<U> epToAdd);

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                //_settings?.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract Task Execute(IJobExecutionContext context);

        public async Task<HashSet<U>> ProcessEpisodes(IAsyncEnumerable<T> serverEpisodes, ICollection<U> episodesAlreadyAdded)
        {
            var episodesBeingAdded = new HashSet<U>();

            await foreach (var ep in GetMediaServerEpisodes())
            {
                if (IsIn(ep, episodesAlreadyAdded) || IsIn(ep, episodesBeingAdded))
                {
                    _logger.LogWarning($"Episode {ep.Name} already processed in this update.");
                    continue;
                }
                // Sanity checks
                if (ep.EpisodeNumber == 0) // no check on season number, Season 0 can be Specials
                {
                    _logger.LogWarning($"Episode {ep.Name} has no episode number. Skipping.");
                    continue;
                }

                var existingEpisode = await GetExistingEpisode(ep);
                if (existingEpisode == null)
                {
                    addEpisode(ep, episodesBeingAdded);
                }
                else
                {
                    _logger.LogWarning($"Episode {ep.Name} already exists in database.");
                    // TODO: update if something changed on the media server
                }
            }


            if (episodesBeingAdded.Any())
            {
                await _repo.AddRange((IEnumerable<IMediaServerEpisode>)episodesBeingAdded);
            }
            return episodesBeingAdded;
        }
    }
}
