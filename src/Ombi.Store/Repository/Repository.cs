using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public class Repository<T> : IRepository<T> where T : Entity
    {
        public Repository(IOmbiContext ctx)
        {
            _ctx = ctx;
            _db = _ctx.Set<T>();
        }
        private readonly DbSet<T> _db;
        private readonly IOmbiContext _ctx;

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

        public async Task AddRange(IEnumerable<T> content)
        {
            _db.AddRange(content);
            await _ctx.SaveChangesAsync();
        }

        public async Task<T> Add(T content)
        {
            await _db.AddAsync(content);
            await _ctx.SaveChangesAsync();
            return content;
        }

        public IIncludableQueryable<TEntity, TProperty> Include<TEntity, TProperty>(
            IQueryable<TEntity> source, Expression<Func<TEntity, TProperty>> navigationPropertyPath)
            where TEntity : class
        {
            return source.Include(navigationPropertyPath);
        }
    }
}