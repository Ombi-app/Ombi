using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Ombi.Core.Models;
using Ombi.Helpers;
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
            var plexMovies = _plex.GetAll().Where(x => x.Type == PlexMediaTypeEntity.Movie && x.AddedAt > from && x.AddedAt < to);
            var embyMovies = _emby.GetAll().Where(x => x.Type == EmbyMediaType.Movie && x.AddedAt > from && x.AddedAt < to);
            
            return GetRecentlyAddedMovies(plexMovies, embyMovies).Take(30);
        }

        public IEnumerable<RecentlyAddedMovieModel> GetRecentlyAddedMovies()
        {
            var plexMovies = _plex.GetAll().Where(x => x.Type == PlexMediaTypeEntity.Movie);
            var embyMovies = _emby.GetAll().Where(x => x.Type == EmbyMediaType.Movie);
            return GetRecentlyAddedMovies(plexMovies, embyMovies);
        }

        public IEnumerable<RecentlyAddedTvModel> GetRecentlyAddedTv(DateTime from, DateTime to, bool groupBySeason)
        {
            var plexTv = _plex.GetAll().Include(x => x.Seasons).Include(x => x.Episodes).Where(x => x.Type == PlexMediaTypeEntity.Show && x.AddedAt > from && x.AddedAt < to);
            var embyTv = _emby.GetAll().Include(x => x.Episodes).Where(x => x.Type == EmbyMediaType.Series && x.AddedAt > from && x.AddedAt < to);

            return GetRecentlyAddedTv(plexTv, embyTv, groupBySeason).Take(30);
        }


        public IEnumerable<RecentlyAddedTvModel> GetRecentlyAddedTv(bool groupBySeason)
        {
            var plexTv = _plex.GetAll().Include(x => x.Seasons).Include(x => x.Episodes).Where(x => x.Type == PlexMediaTypeEntity.Show);
            var embyTv = _emby.GetAll().Include(x => x.Episodes).Where(x => x.Type == EmbyMediaType.Series);

            return GetRecentlyAddedTv(plexTv, embyTv, groupBySeason);
        }

        private IEnumerable<RecentlyAddedTvModel> GetRecentlyAddedTv(IQueryable<PlexServerContent> plexTv, IQueryable<EmbyContent> embyTv,
            bool groupBySeason)
        {
            var model = new HashSet<RecentlyAddedTvModel>();
            TransformPlexShows(plexTv, model);
            TransformEmbyShows(embyTv, model);

            if (groupBySeason)
            {
                return model.DistinctBy(x => x.SeasonNumber);
            }

            return model;
        }

        private IEnumerable<RecentlyAddedMovieModel> GetRecentlyAddedMovies(IQueryable<PlexServerContent> plexMovies, IQueryable<EmbyContent> embyMovies)
        {
            var model = new HashSet<RecentlyAddedMovieModel>();
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

        private static void TransformPlexShows(IQueryable<PlexServerContent> plexShows, HashSet<RecentlyAddedTvModel> model)
        {
            foreach (var plex in plexShows)
            {
                foreach (var season in plex.Seasons)
                {
                    foreach (var episode in plex.Episodes)
                    {
                        model.Add(new RecentlyAddedTvModel
                        {
                            Id = plex.Id,
                            ImdbId = plex.ImdbId,
                            TheMovieDbId = plex.TheMovieDbId,
                            AddedAt = plex.AddedAt,
                            Title = plex.Title,
                            Quality = plex.Quality,
                            ReleaseYear = plex.ReleaseYear,
                            TvDbId = plex.TvDbId,
                            EpisodeNumber = episode.EpisodeNumber,
                            SeasonNumber = season.SeasonNumber
                        });
                    }
                }
            }
        }

        private static void TransformEmbyShows(IQueryable<EmbyContent> embyShows, HashSet<RecentlyAddedTvModel> model)
        {
            foreach (var emby in embyShows)
            {
                foreach (var episode in emby.Episodes)
                {
                    model.Add(new RecentlyAddedTvModel
                    {
                        Id = emby.Id,
                        ImdbId = emby.ProviderId,
                        AddedAt = emby.AddedAt,
                        Title = emby.Title,
                        EpisodeNumber = episode.EpisodeNumber,
                        SeasonNumber = episode.SeasonNumber
                    });
                }
            }
        }
    }
}
