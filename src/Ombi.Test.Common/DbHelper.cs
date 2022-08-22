using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Ombi.Core.Tests
{
    public static class DbHelper
    {
        public static DbSet<T> GetQueryableMockDbSet<T>(params T[] sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            return dbSet.Object;
        }

        public static IQueryable<T> GetQueryable<T>(params T[] sourceList) where T : class
        {
            var mock = new Mock<IQueryable<T>>();

            mock.As<IQueryable<T>>().Setup(x => x.ToListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(sourceList.ToList());

            return mock.Object;

        }
    }
}