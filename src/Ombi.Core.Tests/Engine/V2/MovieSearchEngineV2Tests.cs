using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using Ombi.Api.External.ExternalApis.TheMovieDb;
using Ombi.Api.External.ExternalApis.TheMovieDb.Models;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Engine.V2;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
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
using System.Net.Http;

namespace Ombi.Core.Tests.Engine.V2
{
    [TestFixture]
    public class MovieSearchEngineV2Tests
    {
        private MovieSearchEngineV2 _subject;

        private Mock<ICurrentUser> _currentUser;
        private Mock<IMovieDbApi> _movieApi;
        private Mock<IMapper> _mapper;
        private Mock<IRuleEvaluator> _ruleEvaluator;
        private Mock<ICacheService> _cache;
        private Mock<ISettingsService<OmbiSettings>> _ombiSettings;
        private Mock<ISettingsService<CustomizationSettings>> _customizationSettings;
        private Mock<IMovieRequestEngine> _movieRequestEngine;
        private Mock<IRequestServiceMain> _requestService;
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

            _movieRequestEngine = new Mock<IMovieRequestEngine>();

            _movieRepository = new Mock<IMovieRequestRepository>();
            _movieRepository.Setup(x => x.GetAll())
                .Returns(new List<MovieRequests>().AsQueryable().BuildMock());

            _requestService = new Mock<IRequestServiceMain>();
            _requestService.Setup(x => x.MovieRequestService).Returns(_movieRepository.Object);

            var um = MockHelper.MockUserManager(new List<OmbiUser>());
            var requestSub = new Mock<IRepository<RequestSubscription>>();
            var logger = new Mock<ILogger<MovieSearchEngineV2>>();
            var httpClientFactory = new Mock<IHttpClientFactory>();
            _featureService = new Mock<IFeatureService>();

            _subject = new MovieSearchEngineV2(
                _currentUser.Object,
                _requestService.Object,
                _movieApi.Object,
                _mapper.Object,
                logger.Object,
                _ruleEvaluator.Object,
                um.Object,
                _cache.Object,
                _ombiSettings.Object,
                requestSub.Object,
                _customizationSettings.Object,
                _movieRequestEngine.Object,
                httpClientFactory.Object,
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

        #region RecentlyRequestedMovies

        [Test]
        public async Task RecentlyRequestedMovies_CallsGetMovieInformation_ForEachMovieInCollection()
        {
            // Arrange
            var movieRequests = new List<MovieRequests>
            {
                new MovieRequests { TheMovieDbId = 10 },
                new MovieRequests { TheMovieDbId = 20 },
                new MovieRequests { TheMovieDbId = 30 },
            };

            SetupCachePassThrough<RequestsViewModel<MovieRequests>>();
            SetupCachePassThrough<List<MovieResponseDto>>();
            SetupCachePassThrough<SearchMovieViewModel>();

            _movieRequestEngine
                .Setup(x => x.GetRequests(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<OrderFilterModel>()))
                .ReturnsAsync(new RequestsViewModel<MovieRequests> { Collection = movieRequests });

            _movieApi.Setup(x => x.GetMovieInformation(It.IsAny<int>()))
                .ReturnsAsync((int id) => new MovieResponseDto { Id = id, Title = $"Movie {id}", ImdbId = $"tt{id}" });

            _mapper.Setup(x => x.Map<SearchMovieViewModel>(It.IsAny<MovieResponseDto>()))
                .Returns((MovieResponseDto dto) => new SearchMovieViewModel
                {
                    Id = dto.Id,
                    Title = dto.Title,
                    ImdbId = dto.ImdbId
                });

            // Act
            var results = await _subject.RecentlyRequestedMovies(0, 3, CancellationToken.None);

            // Assert
            _movieApi.Verify(x => x.GetMovieInformation(It.IsAny<int>()), Times.Exactly(3));
            _movieApi.Verify(x => x.GetMovieInformation(10), Times.Once);
            _movieApi.Verify(x => x.GetMovieInformation(20), Times.Once);
            _movieApi.Verify(x => x.GetMovieInformation(30), Times.Once);
        }

        [Test]
        public async Task RecentlyRequestedMovies_ReturnsAllMovies_WhenHideAvailableIsDisabled()
        {
            // Arrange
            var movieRequests = new List<MovieRequests>
            {
                new MovieRequests { TheMovieDbId = 1 },
                new MovieRequests { TheMovieDbId = 2 },
            };

            SetupCachePassThrough<RequestsViewModel<MovieRequests>>();
            SetupCachePassThrough<List<MovieResponseDto>>();

            _movieRequestEngine
                .Setup(x => x.GetRequests(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<OrderFilterModel>()))
                .ReturnsAsync(new RequestsViewModel<MovieRequests> { Collection = movieRequests });

            _movieApi.Setup(x => x.GetMovieInformation(It.IsAny<int>()))
                .ReturnsAsync((int id) => new MovieResponseDto { Id = id, Title = $"Movie {id}", ImdbId = $"tt{id}" });

            _mapper.Setup(x => x.Map<SearchMovieViewModel>(It.IsAny<MovieResponseDto>()))
                .Returns((MovieResponseDto dto) => new SearchMovieViewModel
                {
                    Id = dto.Id,
                    Title = dto.Title,
                    ImdbId = dto.ImdbId,
                    Available = true // all available
                });

            _customizationSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new CustomizationSettings
            {
                HideAvailableFromDiscover = false
            });

            // Act
            var results = (await _subject.RecentlyRequestedMovies(0, 2, CancellationToken.None)).ToList();

            // Assert
            Assert.That(results, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task RecentlyRequestedMovies_ReturnsEmptyList_WhenCollectionIsEmpty()
        {
            // Arrange
            SetupCachePassThrough<RequestsViewModel<MovieRequests>>();
            SetupCachePassThrough<List<MovieResponseDto>>();

            _movieRequestEngine
                .Setup(x => x.GetRequests(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<OrderFilterModel>()))
                .ReturnsAsync(new RequestsViewModel<MovieRequests> { Collection = new List<MovieRequests>() });

            // Act
            var results = (await _subject.RecentlyRequestedMovies(0, 10, CancellationToken.None)).ToList();

            // Assert
            Assert.That(results, Is.Empty);
            _movieApi.Verify(x => x.GetMovieInformation(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task RecentlyRequestedMovies_ReturnsSingleMovie_WhenCollectionHasOneItem()
        {
            // Arrange - regression: single-item parallel execution should work identically
            var movieRequests = new List<MovieRequests>
            {
                new MovieRequests { TheMovieDbId = 42 }
            };

            SetupCachePassThrough<RequestsViewModel<MovieRequests>>();
            SetupCachePassThrough<List<MovieResponseDto>>();

            _movieRequestEngine
                .Setup(x => x.GetRequests(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<OrderFilterModel>()))
                .ReturnsAsync(new RequestsViewModel<MovieRequests> { Collection = movieRequests });

            _movieApi.Setup(x => x.GetMovieInformation(42))
                .ReturnsAsync(new MovieResponseDto { Id = 42, Title = "Solo Movie", ImdbId = "tt0042" });

            _mapper.Setup(x => x.Map<SearchMovieViewModel>(It.IsAny<MovieResponseDto>()))
                .Returns((MovieResponseDto dto) => new SearchMovieViewModel
                {
                    Id = dto.Id,
                    Title = dto.Title,
                    ImdbId = dto.ImdbId
                });

            // Act
            var results = (await _subject.RecentlyRequestedMovies(0, 1, CancellationToken.None)).ToList();

            // Assert
            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0].Title, Is.EqualTo("Solo Movie"));
        }

        #endregion

        #region TransformMovieResultsToResponse (via RecentlyRequestedMovies)

        [Test]
        public async Task TransformMovieResultsToResponse_FiltersAvailableMovies_WhenHideAvailableFromDiscoverIsTrue()
        {
            // Arrange
            var movieRequests = new List<MovieRequests>
            {
                new MovieRequests { TheMovieDbId = 1 },
                new MovieRequests { TheMovieDbId = 2 },
            };

            SetupCachePassThrough<RequestsViewModel<MovieRequests>>();
            SetupCachePassThrough<List<MovieResponseDto>>();

            _movieRequestEngine
                .Setup(x => x.GetRequests(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<OrderFilterModel>()))
                .ReturnsAsync(new RequestsViewModel<MovieRequests> { Collection = movieRequests });

            _movieApi.Setup(x => x.GetMovieInformation(It.IsAny<int>()))
                .ReturnsAsync((int id) => new MovieResponseDto { Id = id, Title = $"Movie {id}", ImdbId = $"tt{id}" });

            // Movie 1: available, Movie 2: not available
            _mapper.Setup(x => x.Map<SearchMovieViewModel>(It.Is<MovieResponseDto>(d => d.Id == 1)))
                .Returns(new SearchMovieViewModel { Id = 1, Title = "Available Movie", ImdbId = "tt1", Available = true });
            _mapper.Setup(x => x.Map<SearchMovieViewModel>(It.Is<MovieResponseDto>(d => d.Id == 2)))
                .Returns(new SearchMovieViewModel { Id = 2, Title = "Unavailable Movie", ImdbId = "tt2", Available = false });

            _customizationSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new CustomizationSettings
            {
                HideAvailableFromDiscover = true
            });

            // Act
            var results = (await _subject.RecentlyRequestedMovies(0, 2, CancellationToken.None)).ToList();

            // Assert - the available movie should be filtered out
            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0].Title, Is.EqualTo("Unavailable Movie"));
        }

        [Test]
        public async Task TransformMovieResultsToResponse_IncludesAvailableMovies_WhenHideAvailableFromDiscoverIsFalse()
        {
            // Arrange
            var movieRequests = new List<MovieRequests>
            {
                new MovieRequests { TheMovieDbId = 1 },
                new MovieRequests { TheMovieDbId = 2 },
            };

            SetupCachePassThrough<RequestsViewModel<MovieRequests>>();
            SetupCachePassThrough<List<MovieResponseDto>>();

            _movieRequestEngine
                .Setup(x => x.GetRequests(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<OrderFilterModel>()))
                .ReturnsAsync(new RequestsViewModel<MovieRequests> { Collection = movieRequests });

            _movieApi.Setup(x => x.GetMovieInformation(It.IsAny<int>()))
                .ReturnsAsync((int id) => new MovieResponseDto { Id = id, Title = $"Movie {id}", ImdbId = $"tt{id}" });

            _mapper.Setup(x => x.Map<SearchMovieViewModel>(It.IsAny<MovieResponseDto>()))
                .Returns((MovieResponseDto dto) => new SearchMovieViewModel
                {
                    Id = dto.Id,
                    Title = dto.Title,
                    ImdbId = dto.ImdbId,
                    Available = true
                });

            _customizationSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new CustomizationSettings
            {
                HideAvailableFromDiscover = false
            });

            // Act
            var results = (await _subject.RecentlyRequestedMovies(0, 2, CancellationToken.None)).ToList();

            // Assert - all movies should be returned even though they are available
            Assert.That(results, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task TransformMovieResultsToResponse_PreWarmsMovieRequests_BeforeParallelProcessing()
        {
            // Arrange - verifies that GetMovieRequests (via MovieRepository.GetAll) is called
            // before processing begins to avoid concurrent EF Core DbContext access.
            var movieRequests = new List<MovieRequests>
            {
                new MovieRequests { TheMovieDbId = 1 }
            };

            SetupCachePassThrough<RequestsViewModel<MovieRequests>>();
            SetupCachePassThrough<List<MovieResponseDto>>();

            _movieRequestEngine
                .Setup(x => x.GetRequests(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<OrderFilterModel>()))
                .ReturnsAsync(new RequestsViewModel<MovieRequests> { Collection = movieRequests });

            _movieApi.Setup(x => x.GetMovieInformation(1))
                .ReturnsAsync(new MovieResponseDto { Id = 1, Title = "Test Movie", ImdbId = "tt1" });

            _mapper.Setup(x => x.Map<SearchMovieViewModel>(It.IsAny<MovieResponseDto>()))
                .Returns((MovieResponseDto dto) => new SearchMovieViewModel { Id = dto.Id, Title = dto.Title, ImdbId = dto.ImdbId });

            // Act - should not throw even though processing happens in parallel
            Assert.DoesNotThrowAsync(async () =>
                await _subject.RecentlyRequestedMovies(0, 1, CancellationToken.None));

            // Verify MovieRepository.GetAll was called for the pre-warm
            _movieRepository.Verify(x => x.GetAll(), Times.AtLeastOnce);
        }

        [Test]
        public async Task TransformMovieResultsToResponse_AllAvailableFiltered_ReturnsEmpty_WhenHideAvailableTrue()
        {
            // Arrange - boundary case: all movies available, all should be filtered
            var movieRequests = new List<MovieRequests>
            {
                new MovieRequests { TheMovieDbId = 1 },
                new MovieRequests { TheMovieDbId = 2 },
                new MovieRequests { TheMovieDbId = 3 },
            };

            SetupCachePassThrough<RequestsViewModel<MovieRequests>>();
            SetupCachePassThrough<List<MovieResponseDto>>();

            _movieRequestEngine
                .Setup(x => x.GetRequests(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<OrderFilterModel>()))
                .ReturnsAsync(new RequestsViewModel<MovieRequests> { Collection = movieRequests });

            _movieApi.Setup(x => x.GetMovieInformation(It.IsAny<int>()))
                .ReturnsAsync((int id) => new MovieResponseDto { Id = id, Title = $"Movie {id}", ImdbId = $"tt{id}" });

            _mapper.Setup(x => x.Map<SearchMovieViewModel>(It.IsAny<MovieResponseDto>()))
                .Returns((MovieResponseDto dto) => new SearchMovieViewModel
                {
                    Id = dto.Id,
                    Title = dto.Title,
                    ImdbId = dto.ImdbId,
                    Available = true
                });

            _customizationSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new CustomizationSettings
            {
                HideAvailableFromDiscover = true
            });

            // Act
            var results = (await _subject.RecentlyRequestedMovies(0, 3, CancellationToken.None)).ToList();

            // Assert
            Assert.That(results, Is.Empty);
        }

        #endregion
    }
}