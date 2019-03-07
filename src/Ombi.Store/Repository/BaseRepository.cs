using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Nito.AsyncEx;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public class BaseRepository<T, U> : IRepository<T> where T : Entity where U : IDbContext
    {
        public BaseRepository(U ctx)
        {
            _ctx = ctx;
            _db = _ctx.Set<T>();
        }
        public DbSet<T> _db { get; }
        private readonly U _ctx;
        private readonly AsyncLock _mutex = new AsyncLock();

        public async Task<T> Find(object key)
        {
            return await _db.FindAsync(key);
        }

        public IQueryable<T> GetAll()
        {
            return _db.AsQueryable();
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T,bool>> predicate)
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
            await _ctx.Database.ExecuteSqlCommandAsync(sql);
        }

        private async Task<int> InternalSaveChanges()
        {
            using (await _mutex.LockAsync())
            {
                return await _ctx.SaveChangesAsync();
            }
        }
        

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _ctx?.Dispose();
            }
            
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}