using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Helpers;
using Ombi.Store.Context;
using Ombi.Store.Entities.Requests;

namespace Ombi.Store.Repository.Requests
{
    public class MovieRequestRepository : Repository<MovieRequests>, IMovieRequestRepository
    {
        public MovieRequestRepository(IOmbiContext ctx) : base(ctx)
        {
            Db = ctx;
        }

        private IOmbiContext Db { get; }

        public async Task<MovieRequests> GetRequestAsync(int theMovieDbId)
        {
            try
            {
                return await Db.MovieRequests.Where(x => x.TheMovieDbId == theMovieDbId)
                    .Include(x => x.RequestedUser)
                    .FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public IQueryable<MovieRequests> GetAll(string userId)
        {
            return GetWithUser().Where(x => x.RequestedUserId == userId);
        }

        public MovieRequests GetRequest(int theMovieDbId)
        {
            return Db.MovieRequests.Where(x => x.TheMovieDbId == theMovieDbId)
                .Include(x => x.RequestedUser)
                .FirstOrDefault();
        }

        public IQueryable<MovieRequests> GetWithUser()
        {
            return Db.MovieRequests
                .Include(x => x.RequestedUser)
                .ThenInclude(x => x.NotificationUserIds)
                .AsQueryable();
        }

        public async Task MarkAsAvailable(int id)
        {
            var movieRequest = new MovieRequests{ Id = id, Available = true, MarkedAsAvailable = DateTime.UtcNow};
            var attached = Db.MovieRequests.Attach(movieRequest);
            attached.Property(x => x.Available).IsModified = true;
            attached.Property(x => x.MarkedAsAvailable).IsModified = true;
            await Db.SaveChangesAsync();
        }

        public IQueryable<MovieRequests> GetWithUser(string userId)
        {
            return Db.MovieRequests
                .Where(x => x.RequestedUserId == userId)
                .Include(x => x.RequestedUser)
                .ThenInclude(x => x.NotificationUserIds)
                .AsQueryable();
        }

        public async Task Update(MovieRequests request)
        {
            if (Db.Entry(request).State == EntityState.Detached)
            {
                Db.MovieRequests.Attach(request);
                Db.Update(request);
            }
            await InternalSaveChanges();
        }

        public async Task Save()
        {
            await InternalSaveChanges();
        }
    }
}