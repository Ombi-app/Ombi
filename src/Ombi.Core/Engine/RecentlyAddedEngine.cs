using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Models;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using RecentlyAddedType = Ombi.Store.Entities.RecentlyAddedType;

namespace Ombi.Core.Engine
{
    public class RecentlyAddedEngine : IRecentlyAddedEngine
    {
        public RecentlyAddedEngine(IPlexContentRepository plex, IEmbyContentRepository emby, IJellyfinContentRepository jellyfin, IRepository<RecentlyAddedLog> recentlyAdded)
        {
            _plex = plex;
            _emby = emby;
            _jellyfin = jellyfin;
            _recentlyAddedLog = recentlyAdded;
        }

        private readonly IPlexContentRepository _plex;
        private readonly IEmbyContentRepository _emby;
        private readonly IJellyfinContentRepository _jellyfin;
        private readonly IRepository<RecentlyAddedLog> _recentlyAddedLog;

        

        public IEnumerable<RecentlyAddedMovieModel> GetRecentlyAddedMovies(DateTime from, DateTime to)
        {
            var plexMovies = _plex.GetAll().Where(x => x.Type == PlexMediaTypeEntity.Movie && x.AddedAt > from && x.AddedAt < to);
            var embyMovies = _emby.GetAll().Where(x => x.Type == EmbyMediaType.Movie && x.AddedAt > from && x.AddedAt < to);
            var jellyfinMovies = _jellyfin.GetAll().Where(x => x.Type == JellyfinMediaType.Movie && x.AddedAt > from && x.AddedAt < to);
            
            return GetRecentlyAddedMovies(plexMovies, embyMovies, jellyfinMovies).Take(30);
        }

        public IEnumerable<RecentlyAddedMovieModel> GetRecentlyAddedMovies()
        {
            var plexMovies = _plex.GetAll().Where(x => x.Type == PlexMediaTypeEntity.Movie);
            var embyMovies = _emby.GetAll().Where(x => x.Type == EmbyMediaType.Movie);
            var jellyfinMovies = _jellyfin.GetAll().Where(x => x.Type == JellyfinMediaType.Movie);
            return GetRecentlyAddedMovies(plexMovies, embyMovies, jellyfinMovies);
        }

        public IEnumerable<RecentlyAddedTvModel> GetRecentlyAddedTv(DateTime from, DateTime to, bool groupBySeason)
        {
            var plexTv = _plex.GetAll().Include(x => x.Seasons).Include(x => x.Episodes).Where(x => x.Type == PlexMediaTypeEntity.Show && x.AddedAt > from && x.AddedAt < to);
            var embyTv = _emby.GetAll().Include(x => x.Episodes).Where(x => x.Type == EmbyMediaType.Series && x.AddedAt > from && x.AddedAt < to);
            var jellyfinTv = _jellyfin.GetAll().Include(x => x.Episodes).Where(x => x.Type == JellyfinMediaType.Series && x.AddedAt > from && x.AddedAt < to);

            return GetRecentlyAddedTv(plexTv, embyTv, jellyfinTv, groupBySeason).Take(30);
        }


        public IEnumerable<RecentlyAddedTvModel> GetRecentlyAddedTv(bool groupBySeason)
        {
            var plexTv = _plex.GetAll().Include(x => x.Seasons).Include(x => x.Episodes).Where(x => x.Type == PlexMediaTypeEntity.Show);
            var embyTv = _emby.GetAll().Include(x => x.Episodes).Where(x => x.Type == EmbyMediaType.Series);
            var jellyfinTv = _jellyfin.GetAll().Include(x => x.Episodes).Where(x => x.Type == JellyfinMediaType.Series);

            return GetRecentlyAddedTv(plexTv, embyTv, jellyfinTv, groupBySeason);
        }

        public async Task<bool> UpdateRecentlyAddedDatabase()
        {
            var plexContent = _plex.GetAll().Include(x => x.Episodes);
            var embyContent = _emby.GetAll().Include(x => x.Episodes);
            var jellyfinContent = _jellyfin.GetAll().Include(x => x.Episodes);
            var recentlyAddedLog = new HashSet<RecentlyAddedLog>();
            foreach (var p in plexContent)
            {
                if (!p.HasTheMovieDb)
                {
                    continue;
                }
                if (p.Type == PlexMediaTypeEntity.Movie)
                {
                    recentlyAddedLog.Add(new RecentlyAddedLog
                    {
                        AddedAt = DateTime.Now,
                        Type = RecentlyAddedType.Plex,
                        ContentId = int.Parse(p.TheMovieDbId),
                        ContentType = ContentType.Parent
                    });
                }
                else
                {
                    // Add the episodes
                    foreach (var ep in p.Episodes)
                    {
                        if (!ep.Series.HasTvDb)
                        {
                            continue;
                        }
                        recentlyAddedLog.Add(new RecentlyAddedLog
                        {
                            AddedAt = DateTime.Now,
                            Type = RecentlyAddedType.Plex,
                            ContentId = int.Parse(ep.Series.TvDbId),
                            ContentType = ContentType.Episode,
                            EpisodeNumber = ep.EpisodeNumber,
                            SeasonNumber = ep.SeasonNumber
                        });
                    }
                }
            }

            foreach (var e in embyContent)
            {
                if (e.TheMovieDbId.IsNullOrEmpty())
                {
                    continue;
                }
                if (e.Type == EmbyMediaType.Movie)
                {
                    recentlyAddedLog.Add(new RecentlyAddedLog
                    {
                        AddedAt = DateTime.Now,
                        Type = RecentlyAddedType.Emby,
                        ContentId = int.Parse(e.TheMovieDbId),
                        ContentType = ContentType.Parent
                    });
                }
                else
                {
                    // Add the episodes
                    foreach (var ep in e.Episodes)
                    {
                        if (ep.Series.TvDbId.IsNullOrEmpty())
                        {
                            continue;
                        }
                        recentlyAddedLog.Add(new RecentlyAddedLog
                        {
                            AddedAt = DateTime.Now,
                            Type = RecentlyAddedType.Emby,
                            ContentId = int.Parse(ep.Series.TvDbId),
                            ContentType = ContentType.Episode,
                            EpisodeNumber = ep.EpisodeNumber,
                            SeasonNumber = ep.SeasonNumber
                        });
                    }
                }
            }

            foreach (var e in jellyfinContent)
            {
                if (e.TheMovieDbId.IsNullOrEmpty())
                {
                    continue;
                }
                if (e.Type == JellyfinMediaType.Movie)
                {
                    recentlyAddedLog.Add(new RecentlyAddedLog
                    {
                        AddedAt = DateTime.Now,
                        Type = RecentlyAddedType.Jellyfin,
                        ContentId = int.Parse(e.TheMovieDbId),
                        ContentType = ContentType.Parent
                    });
                }
                else
                {
                    // Add the episodes
                    foreach (var ep in e.Episodes)
                    {
                        if (ep.Series.TvDbId.IsNullOrEmpty())
                        {
                            continue;
                        }
                        recentlyAddedLog.Add(new RecentlyAddedLog
                        {
                            AddedAt = DateTime.Now,
                            Type = RecentlyAddedType.Jellyfin,
                            ContentId = int.Parse(ep.Series.TvDbId),
                            ContentType = ContentType.Episode,
                            EpisodeNumber = ep.EpisodeNumber,
                            SeasonNumber = ep.SeasonNumber
                        });
                    }
                }
            }
            await _recentlyAddedLog.AddRange(recentlyAddedLog);

            return true;
        }

        private IEnumerable<RecentlyAddedTvModel> GetRecentlyAddedTv(IQueryable<PlexServerContent> plexTv, IQueryable<EmbyContent> embyTv, IQueryable<JellyfinContent> jellyfinTv,
            bool groupBySeason)
        {
            var model = new HashSet<RecentlyAddedTvModel>();
            TransformPlexShows(plexTv, model);
            TransformEmbyShows(embyTv, model);
            TransformJellyfinShows(jellyfinTv, model);

            if (groupBySeason)
            {
                return model.DistinctBy(x => x.SeasonNumber);
            }

            return model;
        }

        private IEnumerable<RecentlyAddedMovieModel> GetRecentlyAddedMovies(IQueryable<PlexServerContent> plexMovies, IQueryable<EmbyContent> embyMovies, IQueryable<JellyfinContent> jellyfinMovies)
        {
            var model = new HashSet<RecentlyAddedMovieModel>();
            TransformPlexMovies(plexMovies, model);
            TransformEmbyMovies(embyMovies, model);
            TransformJellyfinMovies(jellyfinMovies, model);

            return model;
        }

        private static void TransformEmbyMovies(IQueryable<EmbyContent> embyMovies, HashSet<RecentlyAddedMovieModel> model)
        {
            foreach (var emby in embyMovies)
            {
                model.Add(new RecentlyAddedMovieModel
                {
                    Id = emby.Id,
                    ImdbId = emby.ImdbId,
                    TheMovieDbId = emby.TheMovieDbId,
                    TvDbId = emby.TvDbId,
                    AddedAt = emby.AddedAt,
                    Title = emby.Title,
                });
            }
        }

        private static void TransformJellyfinMovies(IQueryable<JellyfinContent> jellyfinMovies, HashSet<RecentlyAddedMovieModel> model)
        {
            foreach (var jellyfin in jellyfinMovies)
            {
                model.Add(new RecentlyAddedMovieModel
                {
                    Id = jellyfin.Id,
                    ImdbId = jellyfin.ImdbId,
                    TheMovieDbId = jellyfin.TheMovieDbId,
                    TvDbId = jellyfin.TvDbId,
                    AddedAt = jellyfin.AddedAt,
                    Title = jellyfin.Title,
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
                        ImdbId = emby.ImdbId,
                        TvDbId = emby.TvDbId,
                        TheMovieDbId = emby.TheMovieDbId,
                        AddedAt = emby.AddedAt,
                        Title = emby.Title,
                        EpisodeNumber = episode.EpisodeNumber,
                        SeasonNumber = episode.SeasonNumber
                    });
                }
            }
        }

        private static void TransformJellyfinShows(IQueryable<JellyfinContent> jellyfinShows, HashSet<RecentlyAddedTvModel> model)
        {
            foreach (var jellyfin in jellyfinShows)
            {
                foreach (var episode in jellyfin.Episodes)
                {
                    model.Add(new RecentlyAddedTvModel
                    {
                        Id = jellyfin.Id,
                        ImdbId = jellyfin.ImdbId,
                        TvDbId = jellyfin.TvDbId,
                        TheMovieDbId = jellyfin.TheMovieDbId,
                        AddedAt = jellyfin.AddedAt,
                        Title = jellyfin.Title,
                        EpisodeNumber = episode.EpisodeNumber,
                        SeasonNumber = episode.SeasonNumber
                    });
                }
            }
        }
    }
}
