using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using Ombi.Api.External.ExternalApis.TheMovieDb;
using Ombi.Api.External.ExternalApis.TheMovieDb.Models;
using Ombi.Api.External.ExternalApis.Trakt;
using Ombi.Api.External.ExternalApis.TvMaze;
using Ombi.Core.Authentication;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Engine.V2;
using Ombi.Core.Helpers;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Mapping.Profiles;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Ombi.Test.Common;

namespace Ombi.Core.Tests.Engine.V2
{
    [TestFixture]
    public class TvSearchEngineV2Tests
    {
        private const int TheMovieDbId = 1399;
        private const int SeasonCount = 2;
        private const int EpisodesPerSeason = 3;

        private TvSearchEngineV2 _engine;
        private Mock<IMovieDbApi> _movieApi;
        private Mock<ISettingsService<CustomizationSettings>> _customizationSettings;
        private CustomizationSettings _customization;
        private List<MovieDbSearchResult> _cachedApiResults;
        private List<SearchTvShowViewModel> _rulesRanAgainst;

        [SetUp]
        public void Setup()
        {
            _customization = new CustomizationSettings { HideAvailableFromDiscover = true };
            _customizationSettings = new Mock<ISettingsService<CustomizationSettings>>();
            _customizationSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(() => _customization);

            // The engine caches the results from TheMovieDb, so every call gets the same instances back
            _cachedApiResults = new List<MovieDbSearchResult>
            {
                new MovieDbSearchResult { Id = TheMovieDbId, Title = "Game of Thrones" }
            };

            _movieApi = new Mock<IMovieDbApi>();
            _movieApi.Setup(x => x.PopularTv(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_cachedApiResults);
            _movieApi.Setup(x => x.GetTVInfo(TheMovieDbId.ToString(), It.IsAny<string>()))
                .ReturnsAsync(new TvInfo
                {
                    id = TheMovieDbId,
                    name = "Game of Thrones",
                    seasons = Enumerable.Range(0, SeasonCount + 1)
                        .Select(x => new Season { season_number = x })
                        .ToList()
                });
            _movieApi.Setup(x => x.GetSeasonEpisodes(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
                .ReturnsAsync((int showId, int seasonNumber, CancellationToken _, string __) => new SeasonDetails
                {
                    season_number = seasonNumber,
                    episodes = Enumerable.Range(1, EpisodesPerSeason)
                        .Select(x => new Episode
                        {
                            season_number = seasonNumber,
                            episode_number = x,
                            name = $"{showId}-S{seasonNumber}E{x}",
                            air_date = "2011-04-17"
                        }).ToArray()
                });

            _rulesRanAgainst = new List<SearchTvShowViewModel>();
            var rules = new Mock<IRuleEvaluator>();
            rules.Setup(x => x.StartSearchRules(It.IsAny<SearchViewModel>()))
                .ReturnsAsync((SearchViewModel model) =>
                {
                    // Capture a snapshot of what the availability rules were given
                    var tv = (SearchTvShowViewModel)model;
                    _rulesRanAgainst.Add(new SearchTvShowViewModel
                    {
                        Id = tv.Id,
                        SeasonRequests = tv.SeasonRequests.Select(s => new SeasonRequests
                        {
                            SeasonNumber = s.SeasonNumber,
                            Episodes = s.Episodes.ToList()
                        }).ToList()
                    });
                    return new List<RuleResult>();
                });

            var currentUser = new Mock<ICurrentUser>();
            currentUser.Setup(x => x.GetUser()).ReturnsAsync((OmbiUser)null);

            var tvRepo = new Mock<ITvRequestRepository>();
            tvRepo.Setup(x => x.Get()).Returns(new List<TvRequests>().AsQueryable().BuildMock());
            var requestService = new Mock<IRequestServiceMain>();
            requestService.Setup(x => x.TvRequestService).Returns(tvRepo.Object);

            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<TvProfile>()).CreateMapper();

            _engine = new TvSearchEngineV2(currentUser.Object, requestService.Object, new Mock<ITvMazeApi>().Object,
                mapper, new Mock<ITraktApi>().Object, rules.Object,
                MockHelper.MockUserManager(new List<OmbiUser>()).Object, new TestCacheService(),
                new Mock<ISettingsService<OmbiSettings>>().Object, new Mock<IRepository<RequestSubscription>>().Object,
                _movieApi.Object, _customizationSettings.Object, new Mock<ITvRequestEngine>().Object,
                new Mock<IFeatureService>().Object);
        }

        [Test]
        public async Task Popular_DoesNotWriteSeasonsOntoTheCachedApiResults()
        {
            await _engine.Popular(0, 10);

            Assert.That(_cachedApiResults[0].SeasonRequests, Is.Empty);
        }

        [Test]
        public async Task Popular_DoesNotDuplicateEpisodes_WhenCalledRepeatedly()
        {
            // The response cache is purged whenever a request is added/approved/denied, so the
            // engine runs again against the same cached results from TheMovieDb
            await _engine.Popular(0, 10);
            await _engine.Popular(0, 10);
            await _engine.Popular(0, 10);

            Assert.That(_rulesRanAgainst, Has.Count.EqualTo(3));
            foreach (var ranAgainst in _rulesRanAgainst)
            {
                Assert.That(ranAgainst.SeasonRequests, Has.Count.EqualTo(SeasonCount));
                Assert.That(ranAgainst.SeasonRequests.SelectMany(x => x.Episodes).Count(),
                    Is.EqualTo(SeasonCount * EpisodesPerSeason));
            }
        }

        [Test]
        public async Task Popular_GivesTheSeasonsToTheAvailabilityRules()
        {
            await _engine.Popular(0, 10);

            var ranAgainst = _rulesRanAgainst.Single();
            // Season 0 (specials) is skipped
            Assert.That(ranAgainst.SeasonRequests.Select(x => x.SeasonNumber), Is.EquivalentTo(new[] { 1, 2 }));
            Assert.That(ranAgainst.SeasonRequests.SelectMany(x => x.Episodes).Count(),
                Is.EqualTo(SeasonCount * EpisodesPerSeason));
        }

        [Test]
        public async Task Popular_DoesNotReturnTheSeasonsToTheClient()
        {
            var result = await _engine.Popular(0, 10);

            Assert.That(result.Single().SeasonRequests, Is.Empty);
        }

        [Test]
        public async Task Popular_DoesNotMixUpTheEpisodesOfShowsWithColidingCacheKeys()
        {
            // Show 1 season 23 and show 12 season 3 both used to build the key "SeasonEpisodes123"
            _cachedApiResults.Clear();
            _cachedApiResults.Add(new MovieDbSearchResult { Id = 1, Title = "First" });
            _cachedApiResults.Add(new MovieDbSearchResult { Id = 12, Title = "Second" });
            SetupShow(1, 23);
            SetupShow(12, 3);

            await _engine.Popular(0, 10);

            var first = _rulesRanAgainst.Single(x => x.Id == 1);
            var second = _rulesRanAgainst.Single(x => x.Id == 12);
            Assert.That(first.SeasonRequests.Select(x => x.SeasonNumber), Is.EquivalentTo(Enumerable.Range(1, 23)));
            Assert.That(second.SeasonRequests.Select(x => x.SeasonNumber), Is.EquivalentTo(Enumerable.Range(1, 3)));
            Assert.That(first.SeasonRequests.SelectMany(x => x.Episodes).Select(x => x.Title),
                Has.All.StartWith("1-"));
            Assert.That(second.SeasonRequests.SelectMany(x => x.Episodes).Select(x => x.Title),
                Has.All.StartWith("12-"));
        }

        [Test]
        public async Task Popular_DoesNotLookupSeasons_WhenNotHidingAvailableContent()
        {
            _customization.HideAvailableFromDiscover = false;

            var result = await _engine.Popular(0, 10);

            Assert.That(result.Single().SeasonRequests, Is.Empty);
            _movieApi.Verify(x => x.GetSeasonEpisodes(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(),
                It.IsAny<string>()), Times.Never);
        }

        private void SetupShow(int theMovieDbId, int seasonCount)
        {
            _movieApi.Setup(x => x.GetTVInfo(theMovieDbId.ToString(), It.IsAny<string>()))
                .ReturnsAsync(new TvInfo
                {
                    id = theMovieDbId,
                    name = $"Show {theMovieDbId}",
                    seasons = Enumerable.Range(0, seasonCount + 1)
                        .Select(x => new Season { season_number = x })
                        .ToList()
                });
        }

        /// <summary>
        /// Mirrors the real cache, the same instances are handed back on every call.
        /// </summary>
        private class TestCacheService : ICacheService
        {
            private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

            public async Task<T> GetOrAddAsync<T>(string cacheKey, Func<Task<T>> factory, DateTimeOffset absoluteExpiration = default)
            {
                if (_cache.TryGetValue(cacheKey, out var cached))
                {
                    return (T)cached;
                }

                var result = await factory();
                _cache[cacheKey] = result;
                return result;
            }

            public T GetOrAdd<T>(string cacheKey, Func<T> factory, DateTimeOffset absoluteExpiration)
            {
                if (_cache.TryGetValue(cacheKey, out var cached))
                {
                    return (T)cached;
                }

                var result = factory();
                _cache[cacheKey] = result;
                return result;
            }

            public void Remove(string key) => _cache.Remove(key);
        }
    }
}
