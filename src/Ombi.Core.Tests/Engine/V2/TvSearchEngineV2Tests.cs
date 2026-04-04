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
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Engine.V2;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Core.Models.Search.V2;
using Ombi.Core.Models.UI;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Test.Common;

namespace Ombi.Core.Tests.Engine.V2
{
    [TestFixture]
    public class TvSearchEngineV2Tests
    {
        private TvSearchEngineV2 _subject;

        private Mock<ICurrentUser> _currentUser;
        private Mock<IMovieDbApi> _movieApi;
        private Mock<IMapper> _mapper;
        private Mock<IRuleEvaluator> _ruleEvaluator;
        private Mock<ICacheService> _cache;
        private Mock<ISettingsService<OmbiSettings>> _ombiSettings;
        private Mock<ISettingsService<CustomizationSettings>> _customizationSettings;
        private Mock<ITvRequestEngine> _tvRequestEngine;
        private Mock<IRequestServiceMain> _requestService;
        private Mock<ITvRequestRepository> _tvRepository;
        private Mock<IMovieRequestRepository> _movieRepository;
        private Mock<IFeatureService> _featureService;

        [SetUp]
        public void Setup()
        {
            _currentUser = new Mock<ICurrentUser>();
            _currentUser.Setup(x => x.GetUser()).ReturnsAsync(new OmbiUser
            {
                Language = "en",
                StreamingCountry = "US"
            });
            _currentUser.Setup(x => x.Username).Returns("testuser");

            _movieApi = new Mock<IMovieDbApi>();
            _mapper = new Mock<IMapper>();

            _ruleEvaluator = new Mock<IRuleEvaluator>();
            _ruleEvaluator.Setup(x => x.StartSearchRules(It.IsAny<SearchViewModel>()))
                .ReturnsAsync(new List<RuleResult>());

            _cache = new Mock<ICacheService>();

            _ombiSettings = new Mock<ISettingsService<OmbiSettings>>();
            _ombiSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new OmbiSettings
            {
                DefaultLanguageCode = "en"
            });

            _customizationSettings = new Mock<ISettingsService<CustomizationSettings>>();
            _customizationSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new CustomizationSettings
            {
                HideAvailableFromDiscover = false
            });

            _tvRequestEngine = new Mock<ITvRequestEngine>();

            _tvRepository = new Mock<ITvRequestRepository>();
            _tvRepository.Setup(x => x.Get())
                .Returns(new List<TvRequests>().AsQueryable().BuildMock());

            _movieRepository = new Mock<IMovieRequestRepository>();
            _movieRepository.Setup(x => x.GetAll())
                .Returns(new List<MovieRequests>().AsQueryable().BuildMock());

            _requestService = new Mock<IRequestServiceMain>();
            _requestService.Setup(x => x.TvRequestService).Returns(_tvRepository.Object);
            _requestService.Setup(x => x.MovieRequestService).Returns(_movieRepository.Object);

            var um = MockHelper.MockUserManager(new List<OmbiUser>());
            var requestSub = new Mock<IRepository<RequestSubscription>>();
            var tvMaze = new Mock<ITvMazeApi>();
            var trakt = new Mock<ITraktApi>();
            _featureService = new Mock<IFeatureService>();

            _subject = new TvSearchEngineV2(
                _currentUser.Object,
                _requestService.Object,
                tvMaze.Object,
                _mapper.Object,
                trakt.Object,
                _ruleEvaluator.Object,
                um.Object,
                _cache.Object,
                _ombiSettings.Object,
                requestSub.Object,
                _movieApi.Object,
                _customizationSettings.Object,
                _tvRequestEngine.Object,
                _featureService.Object
            );
        }

        // Configures the cache mock to invoke the factory delegate for the given result type.
        private void SetupCachePassThrough<T>()
        {
            _cache.Setup(x => x.GetOrAddAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<T>>>(),
                    It.IsAny<DateTimeOffset>()))
                .Returns<string, Func<Task<T>>, DateTimeOffset>((key, factory, exp) => factory());
        }

        private SearchTvShowViewModel MakeSearchTvShow(bool available = false, string title = "Test Show") =>
            new SearchTvShowViewModel { Available = available, Title = title };

        private SearchFullInfoTvShowViewModel MakeFullInfoTvShow(int id = 1, string title = "Test Show") =>
            new SearchFullInfoTvShowViewModel { Id = id, Title = title };

        #region RecentlyRequestedShows

        [Test]
        public async Task RecentlyRequestedShows_CallsGetTVInfo_ForEachShowInCollection()
        {
            // Arrange
            var tvRequests = new List<TvRequests>
            {
                new TvRequests { ExternalProviderId = 100 },
                new TvRequests { ExternalProviderId = 200 },
                new TvRequests { ExternalProviderId = 300 },
            };

            SetupCachePassThrough<RequestsViewModel<TvRequests>>();
            SetupCachePassThrough<List<TvInfo>>();

            _tvRequestEngine
                .Setup(x => x.GetRequests(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<OrderFilterModel>()))
                .ReturnsAsync(new RequestsViewModel<TvRequests> { Collection = tvRequests });

            _movieApi.Setup(x => x.GetTVInfo(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string id, string lang) => new TvInfo
                {
                    id = int.Parse(id),
                    name = $"Show {id}",
                    seasons = new List<Season>()
                });

            var fullShows = tvRequests.Select(r => MakeFullInfoTvShow(r.ExternalProviderId)).ToList();
            _mapper.Setup(x => x.Map<List<SearchFullInfoTvShowViewModel>>(It.IsAny<List<TvInfo>>()))
                .Returns(fullShows);

            _mapper.Setup(x => x.Map<SearchTvShowViewModel>(It.IsAny<SearchFullInfoTvShowViewModel>()))
                .Returns((SearchFullInfoTvShowViewModel vm) => new SearchTvShowViewModel { Title = vm.Title });

            // Act
            var results = (await _subject.RecentlyRequestedShows(0, 3, CancellationToken.None)).ToList();

            // Assert - GetTVInfo called once per show
            _movieApi.Verify(x => x.GetTVInfo(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
            _movieApi.Verify(x => x.GetTVInfo("100", It.IsAny<string>()), Times.Once);
            _movieApi.Verify(x => x.GetTVInfo("200", It.IsAny<string>()), Times.Once);
            _movieApi.Verify(x => x.GetTVInfo("300", It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task RecentlyRequestedShows_ReturnsAllShows_WhenRequestsPresent()
        {
            // Arrange
            var tvRequests = new List<TvRequests>
            {
                new TvRequests { ExternalProviderId = 1 },
                new TvRequests { ExternalProviderId = 2 },
            };

            SetupCachePassThrough<RequestsViewModel<TvRequests>>();
            SetupCachePassThrough<List<TvInfo>>();

            _tvRequestEngine
                .Setup(x => x.GetRequests(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<OrderFilterModel>()))
                .ReturnsAsync(new RequestsViewModel<TvRequests> { Collection = tvRequests });

            _movieApi.Setup(x => x.GetTVInfo(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string id, string lang) => new TvInfo
                {
                    id = int.Parse(id),
                    name = $"Show {id}",
                    seasons = new List<Season>()
                });

            var fullShows = tvRequests.Select(r => MakeFullInfoTvShow(r.ExternalProviderId, $"Show {r.ExternalProviderId}")).ToList();
            _mapper.Setup(x => x.Map<List<SearchFullInfoTvShowViewModel>>(It.IsAny<List<TvInfo>>()))
                .Returns(fullShows);

            _mapper.Setup(x => x.Map<SearchTvShowViewModel>(It.IsAny<SearchFullInfoTvShowViewModel>()))
                .Returns((SearchFullInfoTvShowViewModel vm) => new SearchTvShowViewModel { Title = vm.Title });

            // Act
            var results = (await _subject.RecentlyRequestedShows(0, 2, CancellationToken.None)).ToList();

            // Assert
            Assert.That(results, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task RecentlyRequestedShows_ReturnsEmpty_WhenCollectionIsEmpty()
        {
            // Arrange
            SetupCachePassThrough<RequestsViewModel<TvRequests>>();
            SetupCachePassThrough<List<TvInfo>>();

            _tvRequestEngine
                .Setup(x => x.GetRequests(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<OrderFilterModel>()))
                .ReturnsAsync(new RequestsViewModel<TvRequests> { Collection = new List<TvRequests>() });

            _mapper.Setup(x => x.Map<List<SearchFullInfoTvShowViewModel>>(It.IsAny<List<TvInfo>>()))
                .Returns(new List<SearchFullInfoTvShowViewModel>());

            // Act
            var results = (await _subject.RecentlyRequestedShows(0, 10, CancellationToken.None)).ToList();

            // Assert
            Assert.That(results, Is.Empty);
            _movieApi.Verify(x => x.GetTVInfo(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task RecentlyRequestedShows_PreWarmsTvRequests_BeforeParallelProcessResult()
        {
            // Arrange - verifies that GetTvRequests (via TvRepository.Get) is called
            // before parallel ProcessResult to avoid concurrent EF Core DbContext access.
            var tvRequests = new List<TvRequests>
            {
                new TvRequests { ExternalProviderId = 1 }
            };

            SetupCachePassThrough<RequestsViewModel<TvRequests>>();
            SetupCachePassThrough<List<TvInfo>>();

            _tvRequestEngine
                .Setup(x => x.GetRequests(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<OrderFilterModel>()))
                .ReturnsAsync(new RequestsViewModel<TvRequests> { Collection = tvRequests });

            _movieApi.Setup(x => x.GetTVInfo(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new TvInfo { id = 1, name = "Show 1", seasons = new List<Season>() });

            _mapper.Setup(x => x.Map<List<SearchFullInfoTvShowViewModel>>(It.IsAny<List<TvInfo>>()))
                .Returns(new List<SearchFullInfoTvShowViewModel> { MakeFullInfoTvShow(1) });

            _mapper.Setup(x => x.Map<SearchTvShowViewModel>(It.IsAny<SearchFullInfoTvShowViewModel>()))
                .Returns(MakeSearchTvShow());

            // Act - should not throw
            Assert.DoesNotThrowAsync(async () =>
                await _subject.RecentlyRequestedShows(0, 1, CancellationToken.None));

            // Verify TvRepository.Get was called for the pre-warm
            _tvRepository.Verify(x => x.Get(), Times.AtLeastOnce);
        }

        #endregion

        #region ProcessResults (tested via Popular/Anticipated/Trending)

        private void SetupPopularTvApiResult(List<MovieDbSearchResult> items)
        {
            SetupCachePassThrough<List<MovieDbSearchResult>>();
            _movieApi.Setup(x => x.PopularTv(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(items);
        }

        [Test]
        public async Task ProcessResults_FiltersAvailableShows_WhenHideAvailableFromDiscoverIsTrue()
        {
            // Arrange
            var items = new List<MovieDbSearchResult>
            {
                new MovieDbSearchResult { Id = 1, Title = "Available Show" },
                new MovieDbSearchResult { Id = 2, Title = "Unavailable Show" },
            };

            SetupPopularTvApiResult(items);


            // Available Show -> Available=true; Unavailable Show -> Available=false
            _mapper.Setup(x => x.Map<SearchTvShowViewModel>(It.Is<MovieDbSearchResult>(d => d.Id == 1)))
                .Returns(new SearchTvShowViewModel { Available = true, Title = "Available Show" });
            _mapper.Setup(x => x.Map<SearchTvShowViewModel>(It.Is<MovieDbSearchResult>(d => d.Id == 2)))
                .Returns(new SearchTvShowViewModel { Available = false, Title = "Unavailable Show" });

            _customizationSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new CustomizationSettings
            {
                HideAvailableFromDiscover = true
            });

            // Act
            var results = (await _subject.Popular(0, 2)).ToList();

            // Assert - only the unavailable show should be returned
            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0].Title, Is.EqualTo("Unavailable Show"));
        }

        [Test]
        public async Task ProcessResults_IncludesAvailableShows_WhenHideAvailableFromDiscoverIsFalse()
        {
            // Arrange
            var items = new List<MovieDbSearchResult>
            {
                new MovieDbSearchResult { Id = 1, Title = "Show A" },
                new MovieDbSearchResult { Id = 2, Title = "Show B" },
            };

            SetupPopularTvApiResult(items);

            _mapper.Setup(x => x.Map<SearchTvShowViewModel>(It.IsAny<MovieDbSearchResult>()))
                .Returns((MovieDbSearchResult d) => new SearchTvShowViewModel { Available = true, Title = d.Title });

            _customizationSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new CustomizationSettings
            {
                HideAvailableFromDiscover = false
            });

            // Act
            var results = (await _subject.Popular(0, 2)).ToList();

            // Assert - both shows returned regardless of availability
            Assert.That(results, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task ProcessResults_DoesNotFetchSeasonEpisodes_WhenHideAvailableFromDiscoverIsFalse()
        {
            // Arrange
            var items = new List<MovieDbSearchResult>
            {
                new MovieDbSearchResult { Id = 1, Title = "Show 1" },
            };

            SetupPopularTvApiResult(items);

            _mapper.Setup(x => x.Map<SearchTvShowViewModel>(It.IsAny<MovieDbSearchResult>()))
                .Returns(new SearchTvShowViewModel { Title = "Show 1" });

            _customizationSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new CustomizationSettings
            {
                HideAvailableFromDiscover = false
            });

            // Act
            await _subject.Popular(0, 1);

            // Assert - GetTVInfo and GetSeasonEpisodes should NOT be called
            // because HideAvailableFromDiscover=false skips the enrichment block
            _movieApi.Verify(x => x.GetTVInfo(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _movieApi.Verify(
                x => x.GetSeasonEpisodes(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public async Task ProcessResults_FetchesSeasonEpisodes_WhenHideAvailableFromDiscoverIsTrue()
        {
            // Arrange
            var seasons = new List<Season>
            {
                new Season { season_number = 1 },
                new Season { season_number = 2 },
            };

            var tvInfo = new TvInfo { id = 10, name = "Show 1", seasons = seasons };
            var episodesS1 = new SeasonDetails { season_number = 1, episodes = Array.Empty<Episode>() };
            var episodesS2 = new SeasonDetails { season_number = 2, episodes = Array.Empty<Episode>() };

            var items = new List<MovieDbSearchResult>
            {
                new MovieDbSearchResult { Id = 10, Title = "Show 1" },
            };

            // Cache pass-through for PopularTv call
            SetupCachePassThrough<List<MovieDbSearchResult>>();
            _movieApi.Setup(x => x.PopularTv(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(items);

            // Cache pass-through for GetTVInfo (nameof(GetShowInformation) key)
            SetupCachePassThrough<TvInfo>();

            // Cache pass-through for GetSeasonEpisodes
            SetupCachePassThrough<SeasonDetails>();

            _movieApi.Setup(x => x.GetTVInfo("10", It.IsAny<string>()))
                .ReturnsAsync(tvInfo);
            _movieApi.Setup(x => x.GetSeasonEpisodes(10, 1, It.IsAny<CancellationToken>(), It.IsAny<string>()))
                .ReturnsAsync(episodesS1);
            _movieApi.Setup(x => x.GetSeasonEpisodes(10, 2, It.IsAny<CancellationToken>(), It.IsAny<string>()))
                .ReturnsAsync(episodesS2);

            _mapper.Setup(x => x.Map<SearchTvShowViewModel>(It.IsAny<MovieDbSearchResult>()))
                .Returns(new SearchTvShowViewModel { Title = "Show 1", Available = false });

            _customizationSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new CustomizationSettings
            {
                HideAvailableFromDiscover = true
            });

            // Act
            await _subject.Popular(0, 1);

            // Assert - GetSeasonEpisodes should be called for each non-zero season
            _movieApi.Verify(
                x => x.GetSeasonEpisodes(10, 1, It.IsAny<CancellationToken>(), It.IsAny<string>()),
                Times.Once);
            _movieApi.Verify(
                x => x.GetSeasonEpisodes(10, 2, It.IsAny<CancellationToken>(), It.IsAny<string>()),
                Times.Once);
        }

        [Test]
        public async Task ProcessResults_SkipsSeasonZero_WhenFetchingEpisodes()
        {
            // Arrange - season 0 (special episodes) should be skipped during enrichment
            var seasons = new List<Season>
            {
                new Season { season_number = 0 }, // specials - should be excluded
                new Season { season_number = 1 },
            };

            var tvInfo = new TvInfo { id = 5, name = "Show With Specials", seasons = seasons };
            var episodesS1 = new SeasonDetails { season_number = 1, episodes = Array.Empty<Episode>() };

            var items = new List<MovieDbSearchResult>
            {
                new MovieDbSearchResult { Id = 5, Title = "Show With Specials" },
            };

            SetupCachePassThrough<List<MovieDbSearchResult>>();
            _movieApi.Setup(x => x.PopularTv(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(items);

            SetupCachePassThrough<TvInfo>();
            SetupCachePassThrough<SeasonDetails>();

            _movieApi.Setup(x => x.GetTVInfo("5", It.IsAny<string>()))
                .ReturnsAsync(tvInfo);
            _movieApi.Setup(x => x.GetSeasonEpisodes(5, 1, It.IsAny<CancellationToken>(), It.IsAny<string>()))
                .ReturnsAsync(episodesS1);

            _mapper.Setup(x => x.Map<SearchTvShowViewModel>(It.IsAny<MovieDbSearchResult>()))
                .Returns(new SearchTvShowViewModel { Title = "Show With Specials", Available = false });

            _customizationSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new CustomizationSettings
            {
                HideAvailableFromDiscover = true
            });

            // Act
            await _subject.Popular(0, 1);

            // Assert - season 0 should NOT be fetched
            _movieApi.Verify(
                x => x.GetSeasonEpisodes(5, 0, It.IsAny<CancellationToken>(), It.IsAny<string>()),
                Times.Never);
            // season 1 should be fetched
            _movieApi.Verify(
                x => x.GetSeasonEpisodes(5, 1, It.IsAny<CancellationToken>(), It.IsAny<string>()),
                Times.Once);
        }

        [Test]
        public async Task ProcessResults_ReturnsEmpty_WhenAllShowsAreAvailable_AndHideAvailableIsTrue()
        {
            // Arrange - boundary: all shows available, hide=true → empty result
            var items = new List<MovieDbSearchResult>
            {
                new MovieDbSearchResult { Id = 1, Title = "Show 1" },
                new MovieDbSearchResult { Id = 2, Title = "Show 2" },
            };

            SetupPopularTvApiResult(items);

            _mapper.Setup(x => x.Map<SearchTvShowViewModel>(It.IsAny<MovieDbSearchResult>()))
                .Returns((MovieDbSearchResult d) => new SearchTvShowViewModel { Available = true, Title = d.Title });

            _customizationSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new CustomizationSettings
            {
                HideAvailableFromDiscover = true
            });

            // Act
            var results = (await _subject.Popular(0, 2)).ToList();

            // Assert
            Assert.That(results, Is.Empty);
        }

        [Test]
        public async Task ProcessResults_PreWarmsTvRequests_BeforeParallelProcessing()
        {
            // Arrange - verifies TvRepository.Get() is called before parallel ProcessResult
            var items = new List<MovieDbSearchResult>
            {
                new MovieDbSearchResult { Id = 1, Title = "Show 1" },
            };

            SetupPopularTvApiResult(items);

            _mapper.Setup(x => x.Map<SearchTvShowViewModel>(It.IsAny<MovieDbSearchResult>()))
                .Returns(new SearchTvShowViewModel { Title = "Show 1" });

            // Act
            Assert.DoesNotThrowAsync(async () => await _subject.Popular(0, 1));

            // Verify TvRepository.Get was called for the pre-warm
            _tvRepository.Verify(x => x.Get(), Times.AtLeastOnce);
        }

        [Test]
        public async Task ProcessResults_ReturnsEmpty_WhenItemListIsEmpty()
        {
            // Arrange
            SetupPopularTvApiResult(new List<MovieDbSearchResult>());

            // Act
            var results = (await _subject.Popular(0, 5)).ToList();

            // Assert
            Assert.That(results, Is.Empty);
        }

        #endregion
    }
}