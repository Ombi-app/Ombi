using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Ombi.Core.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Engine
{
    public class RecentlyAddedEngine : IRecentlyAddedEngine
    {
        public RecentlyAddedEngine(IPlexContentRepository plex, IEmbyContentRepository emby)
        {
            _plex = plex;
            _emby = emby;
        }

        private readonly IPlexContentRepository _plex;
        private readonly IEmbyContentRepository _emby;

        public IEnumerable<RecentlyAddedMovieModel> GetRecentlyAddedMovies(DateTime from, DateTime to)
        {
            var model = new HashSet<RecentlyAddedMovieModel>();
            var plexMovies = _plex.GetAll().Where(x => x.Type == PlexMediaTypeEntity.Movie && x.AddedAt > from && x.AddedAt < to);
            var embyMovies = _emby.GetAll().Where(x => x.Type == EmbyMediaType.Movie && x.AddedAt > from && x.AddedAt < to);

            TransformPlexMovies(plexMovies, model);
            TransformEmbyMovies(embyMovies, model);

            return model.Take(30);
        }

        public IEnumerable<RecentlyAddedMovieModel> GetRecentlyAddedMovies()
        {
            var model = new HashSet<RecentlyAddedMovieModel>();
            var plexMovies = _plex.GetAll().Where(x => x.Type == PlexMediaTypeEntity.Movie);
            var embyMovies = _emby.GetAll().Where(x => x.Type == EmbyMediaType.Movie);

            TransformPlexMovies(plexMovies, model);
            TransformEmbyMovies(embyMovies, model);

            return model;
        }

        private static void TransformEmbyMovies(IQueryable<EmbyContent> embyMovies, HashSet<RecentlyAddedMovieModel> model)
        {
            foreach (var emby in embyMovies)
            {
                model.Add(new RecentlyAddedMovieModel
                {
                    Id = emby.Id,
                    ImdbId = emby.ProviderId,
                    AddedAt = emby.AddedAt,
                    Title = emby.Title,
                });
            }
        }

        private static void TransformPlexMovies(IQueryable<PlexServerContent> plexMovies, HashSet<RecentlyAddedMovieModel> model)
        {
            foreach (var plex in plexMovies)
            {
                model.Add(new RecentlyAddedMovieModel
                {
                    Id = plex.Id,
                    ImdbId = plex.ImdbId,
                    TheMovieDbId = plex.TheMovieDbId,
                    AddedAt = plex.AddedAt,
                    Title = plex.Title,
                    Quality = plex.Quality,
                    ReleaseYear = plex.ReleaseYear
                });
            }
        }
    }
}
