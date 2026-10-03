using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Ombi.Core.Engine;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Rules;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Tests.Rule.Search
{
    [TestFixture]
    public class SonarrCacheRuleTests
    {
        private sealed class TestExternalContext : ExternalContext
        {
            public TestExternalContext(DbContextOptions options) : base(options)
            {
            }
        }

        private SqliteConnection _connection;
        private TestExternalContext _context;
        private SonarrCacheRule _rule;

        [SetUp]
        public void Setup()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<TestExternalContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new TestExternalContext(options);
            _context.Database.EnsureCreated();
            _rule = new SonarrCacheRule(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }

        [Test]
        public async Task Execute_BaseRequest_NonTvShow_ReturnsSuccess()
        {
            var request = new MovieRequests();

            var result = await _rule.Execute(request);

            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task Execute_BaseRequest_NotInCache_ReturnsSuccess()
        {
            var request = new ChildRequests
            {
                RequestType = RequestType.TvShow,
                Id = 12345,
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        SeasonNumber = 1,
                        Episodes = new List<EpisodeRequests> { new EpisodeRequests { EpisodeNumber = 1 } }
                    }
                }
            };

            var result = await _rule.Execute(request);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, request.SeasonRequests[0].Episodes.Count);
        }

        [Test]
        public async Task Execute_BaseRequest_PartialEpisodesInCache_RemovesCachedEpisodes()
        {
            _context.SonarrCache.Add(new SonarrCache { TheMovieDbId = 100, TvDbId = 200 });
            _context.SonarrEpisodeCache.AddRange(
                new SonarrEpisodeCache { MovieDbId = 100, TvDbId = 200, SeasonNumber = 1, EpisodeNumber = 1, HasFile = false },
                new SonarrEpisodeCache { MovieDbId = 100, TvDbId = 200, SeasonNumber = 1, EpisodeNumber = 2, HasFile = false }
            );
            await _context.SaveChangesAsync();

            var ep1 = new EpisodeRequests { EpisodeNumber = 1 };
            var ep2 = new EpisodeRequests { EpisodeNumber = 2 };
            var ep3 = new EpisodeRequests { EpisodeNumber = 3 };

            var request = new ChildRequests
            {
                RequestType = RequestType.TvShow,
                Id = 100,
                Title = "Test Series",
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        SeasonNumber = 1,
                        Episodes = new List<EpisodeRequests> { ep1, ep2, ep3 }
                    }
                }
            };

            var result = await _rule.Execute(request);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, request.SeasonRequests[0].Episodes.Count);
            Assert.AreEqual(3, request.SeasonRequests[0].Episodes[0].EpisodeNumber);
        }

        [Test]
        public async Task Execute_BaseRequest_AllEpisodesInCache_ReturnsEpisodesAlreadyRequested()
        {
            _context.SonarrCache.Add(new SonarrCache { TheMovieDbId = 100, TvDbId = 200 });
            _context.SonarrEpisodeCache.AddRange(
                new SonarrEpisodeCache { MovieDbId = 100, TvDbId = 200, SeasonNumber = 1, EpisodeNumber = 1, HasFile = true }
            );
            await _context.SaveChangesAsync();

            var request = new ChildRequests
            {
                RequestType = RequestType.TvShow,
                Id = 100,
                Title = "Test Series",
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        SeasonNumber = 1,
                        Episodes = new List<EpisodeRequests> { new EpisodeRequests { EpisodeNumber = 1 } }
                    }
                }
            };

            var result = await _rule.Execute(request);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(ErrorCode.EpisodesAlreadyRequested, result.ErrorCode);
        }

        [Test]
        public async Task Execute_SearchViewModel_NonTvShow_ReturnsSuccess()
        {
            var search = new SearchMovieViewModel();

            var result = await _rule.Execute(search);

            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task Execute_SearchViewModel_NoTvDbId_ReturnsSuccess()
        {
            var search = new SearchTvShowViewModel { TheTvDbId = null };

            var result = await _rule.Execute(search);

            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task Execute_SearchViewModel_NotInCache_LeavesApprovedFalse()
        {
            var search = new SearchTvShowViewModel { TheTvDbId = "999" };

            var result = await _rule.Execute(search);

            Assert.IsTrue(result.Success);
            Assert.IsFalse(search.Approved);
        }

        [Test]
        public async Task Execute_SearchViewModel_InCache_MarksEpisodesApprovedAndAvailable()
        {
            _context.SonarrCache.Add(new SonarrCache { TvDbId = 555, TheMovieDbId = 666 });
            _context.SonarrEpisodeCache.AddRange(
                new SonarrEpisodeCache { TvDbId = 555, SeasonNumber = 1, EpisodeNumber = 1, HasFile = false },
                new SonarrEpisodeCache { TvDbId = 555, SeasonNumber = 1, EpisodeNumber = 2, HasFile = true },
                // Duplicate episode entry with HasFile true should be respected
                new SonarrEpisodeCache { TvDbId = 555, SeasonNumber = 1, EpisodeNumber = 2, HasFile = false }
            );
            await _context.SaveChangesAsync();

            var ep1 = new EpisodeRequests { EpisodeNumber = 1 };
            var ep2 = new EpisodeRequests { EpisodeNumber = 2 };
            var ep3 = new EpisodeRequests { EpisodeNumber = 3 };

            var search = new SearchTvShowViewModel
            {
                TheTvDbId = "555",
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        SeasonNumber = 1,
                        Episodes = new List<EpisodeRequests> { ep1, ep2, ep3 }
                    }
                }
            };

            var result = await _rule.Execute(search);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(search.Approved);
            Assert.IsTrue(ep1.Approved);
            Assert.IsFalse(ep1.Available);
            Assert.IsTrue(ep2.Approved);
            Assert.IsTrue(search.Available);
            Assert.IsFalse(ep3.Approved);
        }
    }
}
