﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IRepository<T> where T : Entity
    {
        Task<T> Find(object key);
        IQueryable<T> GetAll();
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task AddRange(IEnumerable<T> content);
        Task<T> Add(T content);
        Task DeleteRange(IEnumerable<T> req);
        Task Delete(T request);
        Task<int> SaveChangesAsync();

        IIncludableQueryable<TEntity, TProperty> Include<TEntity, TProperty>(
            IQueryable<TEntity> source, Expression<Func<TEntity, TProperty>> navigationPropertyPath)
            where TEntity : class;
    }
}