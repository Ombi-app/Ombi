using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Moq;
using NUnit.Framework;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Rules.Search;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Core.Tests.Rule.Search
{
    public class RadarrCacheRuleTests
    {
        [SetUp]
        public void Setup()
        {
            ContextMock = new Mock<IOmbiContext>();
            Rule = new RadarrCacheRule(ContextMock.Object);

        }

        private RadarrCacheRule Rule { get; set; }
        private Mock<IOmbiContext> ContextMock { get; set; }

        [Test]
        [Ignore("EF IAsyncQueryProvider")]
        public async Task Should_ReturnApproved_WhenMovieIsInRadarr()
        {
            var list = new List<RadarrCache>(){new RadarrCache
            {
                TheMovieDbId = 123
            }}.AsQueryable();
            var radarrMock = new Mock<DbSet<RadarrCache>>();
            radarrMock.As<IAsyncEnumerable<RadarrCache>>()
                .Setup(m => m.GetEnumerator())
                .Returns(new TestAsyncEnumerator<RadarrCache>(list.GetEnumerator()));


            radarrMock.As<IQueryable<RadarrCache>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<RadarrCache>(list.Provider));

            radarrMock.As<IQueryable<RadarrCache>>().Setup(m => m.Expression).Returns(list.Expression);
            radarrMock.As<IQueryable<RadarrCache>>().Setup(m => m.ElementType).Returns(list.ElementType);
            radarrMock.As<IQueryable<RadarrCache>>().Setup(m => m.GetEnumerator()).Returns(() => list.GetEnumerator());


            ContextMock.Setup(c => c.Set<RadarrCache>()).Returns(radarrMock.Object);

            var request = new SearchMovieViewModel { Id = 123 };
            var result =await  Rule.Execute(request);

            Assert.True(result.Success);
            Assert.True(request.Approved);
        }


        [Test]
        [Ignore("EF IAsyncQueryProvider")]
        public async Task Should_ReturnNotApproved_WhenMovieIsNotInRadarr()
        {
            var list = DbHelper.GetQueryableMockDbSet(new RadarrCache
            {
                TheMovieDbId = 000012
            });

            ContextMock.Setup(x => x.RadarrCache).Returns(list);

            var request = new SearchMovieViewModel { Id = 123 };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.False(request.Approved);
        }
    }

    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            return new TestAsyncEnumerable<TResult>(expression);
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute<TResult>(expression));
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetEnumerator()
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider
        {
            get { return new TestAsyncQueryProvider<T>(this); }
        }
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public void Dispose()
        {
            _inner.Dispose();
        }

        public T Current => _inner.Current;

        public Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            return Task.FromResult(_inner.MoveNext());
        }
    }
}
