using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Ombi.Helpers;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Polly;

namespace Ombi.Store.Repository
{
    public class BaseRepository<T, U> : IRepository<T> where T : Entity where U : DbContext
    {
        public BaseRepository(U ctx)
        {
            _ctx = ctx;
            _db = _ctx.Set<T>();
        }
        public DbSet<T> _db { get; }
        private readonly U _ctx;

        public async Task<T> Find(object key)
        {
            return await _db.FindAsync(key);
        }

        public async Task<T> Find(object key, CancellationToken cancellationToken)
        {
            return await _db.FindAsync(new[] { key }, cancellationToken: cancellationToken);
        }

        public IQueryable<T> GetAll()
        {
            return _db.AsQueryable();
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _db.FirstOrDefaultAsync(predicate);
        }

        public async Task AddRange(IEnumerable<T> content, bool save = true)
        {
            _db.AddRange(content);
            if (save)
            {
                await InternalSaveChanges();
            }
        }

        public async Task<T> Add(T content)
        {
            await _db.AddAsync(content);
            await InternalSaveChanges();
            return content;
        }

        public async Task Delete(T request)
        {
            _db.Remove(request);
            await InternalSaveChanges();
        }

        public async Task DeleteRange(IEnumerable<T> req)
        {
            _db.RemoveRange(req);
            await InternalSaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await InternalSaveChanges();
        }

        public IIncludableQueryable<TEntity, TProperty> Include<TEntity, TProperty>(
            IQueryable<TEntity> source, Expression<Func<TEntity, TProperty>> navigationPropertyPath)
            where TEntity : class
        {
            return source.Include(navigationPropertyPath);
        }

        public async Task ExecuteSql(string sql)
        {
            await _ctx.Database.ExecuteSqlRawAsync(sql);
        }

        protected async Task<int> InternalSaveChanges()
        {
            try
            {
                var r = await _ctx.SaveChangesAsync();
                return r;
            }
            catch (DbUpdateException ex)
            {
                // Check if this is a duplicate key constraint violation
                var isDuplicateKey = ex.InnerException?.Message?.Contains("Duplicate entry") == true ||
                                    ex.InnerException?.Message?.Contains("UNIQUE constraint") == true ||
                                    ex.InnerException?.Message?.Contains("duplicate key") == true;

                if (isDuplicateKey)
                {
                    // Clear the change tracker to prevent corruption
                    var duplicateEntries = _ctx.ChangeTracker.Entries()
                        .Where(e => e.State == EntityState.Added)
                        .ToList();

                    // Try to extract information about which entities caused the conflict
                    var duplicateInfo = string.Join(", ", duplicateEntries
                        .Select(e => {
                            var entity = e.Entity as dynamic;
                            try
                            {
                                return $"{e.Entity.GetType().Name} (Key: {entity?.Key})";
                            }
                            catch
                            {
                                return e.Entity.GetType().Name;
                            }
                        })
                        .Take(10));

                    // Clear the change tracker to prevent further corruption
                    foreach (var entry in duplicateEntries)
                    {
                        entry.State = EntityState.Detached;
                    }

                    throw new InvalidOperationException(
                        $"Duplicate key constraint violation. Attempted to add entities that already exist in the database. " +
                        $"Entities: {duplicateInfo}. Original error: {ex.InnerException?.Message ?? ex.Message}",
                        ex);
                }

                // Re-throw if not a duplicate key error
                throw;
            }
        }


        //private bool _disposed;
        //// Protected implementation of Dispose pattern.
        //protected virtual void Dispose(bool disposing)
        //{
        //    if (_disposed)
        //        return;

        //    if (disposing)
        //    {
        //        _ctx?.Dispose();
        //    }

        //    _disposed = true;
        //}

        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}
    }
}