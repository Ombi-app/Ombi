using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Engine;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Requests;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Schedule.Jobs.Plex;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Test.Common;
using Quartz;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class PlexWatchlistImportTests
    {

        private PlexWatchlistImport _subject;
        private AutoMocker _mocker;
        private Mock<IJobExecutionContext> _context;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            var um = MockHelper.MockUserManager(new List<OmbiUser> { new OmbiUser { Id = "abc", UserType = UserType.PlexUser, MediaServerToken = "abc", UserName = "abc", NormalizedUserName = "ABC" } });
            _mocker.Use(um);
            _context = _mocker.GetMock<IJobExecutionContext>();
            _context.Setup(x => x.CancellationToken).Returns(CancellationToken.None);
            _subject = _mocker.CreateInstance<PlexWatchlistImport>();
        }

        [Test]
        public async Task TerminatesWhenPlexIsNotEnabled()
        {
            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings { Enable = false, EnableWatchlistImport = true });
            await _subject.Execute(null);
            _mocker.Verify<IMovieRequestEngine>(x => x.RequestMovie(It.IsAny<MovieRequestViewModel>()), Times.Never);
            _mocker.Verify<IPlexApi>(x => x.GetWatchlist(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _mocker.Verify<IMovieRequestEngine>(x => x.RequestMovie(It.IsAny<MovieRequestViewModel>()), Times.Never);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.GetAll(), Times.Never);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.Add(It.IsAny<PlexWatchlistHistory>()), Times.Never);
        }
        [Test]
        public async Task TerminatesWhenWatchlistIsNotEnabled()
        {
            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings { Enable = true, EnableWatchlistImport = false });
            await _subject.Execute(null);
            _mocker.Verify<IMovieRequestEngine>(x => x.RequestMovie(It.IsAny<MovieRequestViewModel>()), Times.Never);
            _mocker.Verify<IPlexApi>(x => x.GetWatchlist(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _mocker.Verify<IMovieRequestEngine>(x => x.RequestMovie(It.IsAny<MovieRequestViewModel>()), Times.Never);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.GetAll(), Times.Never);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.Add(It.IsAny<PlexWatchlistHistory>()), Times.Never);
        }

        [Test]
        public async Task EmptyWatchList()
        {
            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings { Enable = true, EnableWatchlistImport = true });
            _mocker.Setup<IPlexApi, Task<PlexWatchlistContainer>>(x => x.GetWatchlist(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PlexWatchlistContainer());
            await _subject.Execute(_context.Object);
            _mocker.Verify<IMovieRequestEngine>(x => x.RequestMovie(It.IsAny<MovieRequestViewModel>()), Times.Never);
            _mocker.Verify<IPlexApi>(x => x.GetWatchlist(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _mocker.Verify<IMovieRequestEngine>(x => x.RequestMovie(It.IsAny<MovieRequestViewModel>()), Times.Never);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.GetAll(), Times.Never);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.Add(It.IsAny<PlexWatchlistHistory>()), Times.Never);
        }

        [Test]
        public async Task NoPlexUsersWithToken()
        {
            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings { Enable = true, EnableWatchlistImport = true });
            var um = MockHelper.MockUserManager(new List<OmbiUser>
            {
                new OmbiUser { Id = "abc", UserType = UserType.EmbyUser, MediaServerToken = "abc", UserName = "abc", NormalizedUserName = "ABC" },
                new OmbiUser { Id = "abc", UserType = UserType.LocalUser, MediaServerToken = "abc", UserName = "abc", NormalizedUserName = "ABC" },
                new OmbiUser { Id = "abc", UserType = UserType.SystemUser, MediaServerToken = "abc", UserName = "abc", NormalizedUserName = "ABC" },
                new OmbiUser { Id = "abc", UserType = UserType.JellyfinUser, MediaServerToken = "abc", UserName = "abc", NormalizedUserName = "ABC" },
                new OmbiUser { Id = "abc", UserType = UserType.EmbyConnectUser, MediaServerToken = "abc", UserName = "abc", NormalizedUserName = "ABC" },
                new OmbiUser { Id = "abc", UserType = UserType.PlexUser, UserName = "abc", NormalizedUserName = "ABC" },
            });
            _mocker.Use(um);
            _subject = _mocker.CreateInstance<PlexWatchlistImport>();

            await _subject.Execute(_context.Object);
            _mocker.Verify<IPlexApi>(x => x.GetWatchlist(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _mocker.Verify<IMovieRequestEngine>(x => x.RequestMovie(It.IsAny<MovieRequestViewModel>()), Times.Never);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.GetAll(), Times.Never);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.Add(It.IsAny<PlexWatchlistHistory>()), Times.Never);
        }


        [Test]
        public async Task MultipleUsers()
        {
            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings { Enable = true, EnableWatchlistImport = true });
            var um = MockHelper.MockUserManager(new List<OmbiUser>
            {
                new OmbiUser { Id = "abc1", UserType = UserType.PlexUser, MediaServerToken = "abc1", UserName = "abc1", NormalizedUserName = "ABC1" },
                new OmbiUser { Id = "abc2", UserType = UserType.PlexUser, MediaServerToken = "abc2", UserName = "abc2", NormalizedUserName = "ABC2" },
                new OmbiUser { Id = "abc3", UserType = UserType.PlexUser, MediaServerToken = "abc3", UserName = "abc3", NormalizedUserName = "ABC3" },
                new OmbiUser { Id = "abc4", UserType = UserType.PlexUser, MediaServerToken = "abc4", UserName = "abc4", NormalizedUserName = "ABC4" },
                new OmbiUser { Id = "abc5", UserType = UserType.PlexUser, MediaServerToken = "abc5", UserName = "abc5", NormalizedUserName = "ABC5" },
            });
            _mocker.Use(um);
            _subject = _mocker.CreateInstance<PlexWatchlistImport>();

            await _subject.Execute(_context.Object);
            _mocker.Verify<IPlexApi>(x => x.GetWatchlist(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(5));
            _mocker.Verify<IMovieRequestEngine>(x => x.RequestMovie(It.IsAny<MovieRequestViewModel>()), Times.Never);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.GetAll(), Times.Never);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.Add(It.IsAny<PlexWatchlistHistory>()), Times.Never);
        }


        [Test]
        public async Task MovieRequestFromWatchList_NoGuid()
        {
            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings { Enable = true, EnableWatchlistImport = true });
            _mocker.Setup<IPlexApi, Task<PlexWatchlistContainer>>(x => x.GetWatchlist(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PlexWatchlistContainer
            {
                MediaContainer = new PlexWatchlist
                {
                    Metadata = new List<Metadata>
                    {
                        new Metadata
                        {
                            type = "movie",
                            ratingKey = "abc"
                        }
                    }
                }
            });
            _mocker.Setup<IPlexApi, Task<PlexWatchlistMetadataContainer>>(x => x.GetWatchlistMetadata("abc", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlexWatchlistMetadataContainer
                {
                    MediaContainer = new PlexWatchlistMetadata
                    {
                        Metadata = new WatchlistMetadata[]
                        {
                            new WatchlistMetadata
                            {
                                Guid = new List<PlexGuids>
                                {
                                    new PlexGuids
                                    {
                                        Id = "tmdb://123"
                                    }
                                }
                            }
                        }

                    }
                });
            _mocker.Setup<IMovieRequestEngine, Task<RequestEngineResult>>(x => x.RequestMovie(It.IsAny<MovieRequestViewModel>()))
                .ReturnsAsync(new RequestEngineResult { RequestId = 1 });
            await _subject.Execute(_context.Object);
            _mocker.Verify<IMovieRequestEngine>(x => x.RequestMovie(It.Is<MovieRequestViewModel>(x => x.TheMovieDbId == 123)), Times.Once);
            _mocker.Verify<IPlexApi>(x => x.GetWatchlistMetadata("abc", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _mocker.Verify<IMovieRequestEngine>(x => x.SetUser(It.Is<OmbiUser>(x => x.Id == "abc")), Times.Once);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.GetAll(), Times.Once);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.Add(It.IsAny<PlexWatchlistHistory>()), Times.Once);
        }


        [Test]
        public async Task TvRequestFromWatchList_NoGuid()
        {
            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings { Enable = true, EnableWatchlistImport = true });
            _mocker.Setup<IPlexApi, Task<PlexWatchlistContainer>>(x => x.GetWatchlist(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PlexWatchlistContainer
            {
                MediaContainer = new PlexWatchlist
                {
                    Metadata = new List<Metadata>
                    {
                        new Metadata
                        {
                            type = "show",
                            ratingKey = "abc"
                        }
                    }
                }
            });
            _mocker.Setup<IPlexApi, Task<PlexWatchlistMetadataContainer>>(x => x.GetWatchlistMetadata("abc", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlexWatchlistMetadataContainer
                {
                    MediaContainer = new PlexWatchlistMetadata
                    {
                        Metadata = new WatchlistMetadata[]
                        {
                            new WatchlistMetadata
                            {
                                Guid = new List<PlexGuids>
                                {
                                    new PlexGuids
                                    {
                                        Id = "tmdb://123"
                                    }
                                }
                            }
                        }

                    }
                });
            _mocker.Setup<ITvRequestEngine, Task<RequestEngineResult>>(x => x.RequestTvShow(It.IsAny<TvRequestViewModelV2>()))
                .ReturnsAsync(new RequestEngineResult { RequestId = 1 });
            await _subject.Execute(_context.Object);
            _mocker.Verify<ITvRequestEngine>(x => x.RequestTvShow(It.Is<TvRequestViewModelV2>(x => x.TheMovieDbId == 123)), Times.Once);
            _mocker.Verify<IPlexApi>(x => x.GetWatchlistMetadata("abc", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _mocker.Verify<ITvRequestEngine>(x => x.SetUser(It.Is<OmbiUser>(x => x.Id == "abc")), Times.Once);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.GetAll(), Times.Once);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.Add(It.IsAny<PlexWatchlistHistory>()), Times.Once);
        }

        [Test]
        public async Task MovieRequestFromWatchList_AlreadyRequested()
        {
            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings { Enable = true, EnableWatchlistImport = true });
            _mocker.Setup<IPlexApi, Task<PlexWatchlistContainer>>(x => x.GetWatchlist(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PlexWatchlistContainer
            {
                MediaContainer = new PlexWatchlist
                {
                    Metadata = new List<Metadata>
                    {
                        new Metadata
                        {
                            type = "movie",
                            ratingKey = "abc"
                        }
                    }
                }
            });
            _mocker.Setup<IPlexApi, Task<PlexWatchlistMetadataContainer>>(x => x.GetWatchlistMetadata("abc", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlexWatchlistMetadataContainer
                {
                    MediaContainer = new PlexWatchlistMetadata
                    {
                        Metadata = new WatchlistMetadata[]
                        {
                            new WatchlistMetadata
                            {
                                Guid = new List<PlexGuids>
                                {
                                    new PlexGuids
                                    {
                                        Id = "tmdb://123"
                                    }
                                }
                            }
                        }

                    }
                });
            _mocker.Setup<IMovieRequestEngine, Task<RequestEngineResult>>(x => x.RequestMovie(It.IsAny<MovieRequestViewModel>()))
                .ReturnsAsync(new RequestEngineResult { ErrorCode = ErrorCode.AlreadyRequested, ErrorMessage = "Requested" });
            await _subject.Execute(_context.Object);
            _mocker.Verify<IMovieRequestEngine>(x => x.RequestMovie(It.Is<MovieRequestViewModel>(x => x.TheMovieDbId == 123)), Times.Once);
            _mocker.Verify<IPlexApi>(x => x.GetWatchlistMetadata("abc", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _mocker.Verify<IMovieRequestEngine>(x => x.SetUser(It.Is<OmbiUser>(x => x.Id == "abc")), Times.Once);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.GetAll(), Times.Once);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.Add(It.IsAny<PlexWatchlistHistory>()), Times.Never);
        }

        [Test]
        public async Task TvRequestFromWatchList_AlreadyRequested()
        {
            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings { Enable = true, EnableWatchlistImport = true });
            _mocker.Setup<IPlexApi, Task<PlexWatchlistContainer>>(x => x.GetWatchlist(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PlexWatchlistContainer
            {
                MediaContainer = new PlexWatchlist
                {
                    Metadata = new List<Metadata>
                    {
                        new Metadata
                        {
                            type = "show",
                            ratingKey = "abc"
                        }
                    }
                }
            });
            _mocker.Setup<IPlexApi, Task<PlexWatchlistMetadataContainer>>(x => x.GetWatchlistMetadata("abc", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlexWatchlistMetadataContainer
                {
                    MediaContainer = new PlexWatchlistMetadata
                    {
                        Metadata = new WatchlistMetadata[]
                        {
                            new WatchlistMetadata
                            {
                                Guid = new List<PlexGuids>
                                {
                                    new PlexGuids
                                    {
                                        Id = "tmdb://123"
                                    }
                                }
                            }
                        }

                    }
                });
            _mocker.Setup<ITvRequestEngine, Task<RequestEngineResult>>(x => x.RequestTvShow(It.IsAny<TvRequestViewModelV2>()))
                .ReturnsAsync(new RequestEngineResult { ErrorCode = ErrorCode.AlreadyRequested, ErrorMessage = "Requested" });
            await _subject.Execute(_context.Object);
            _mocker.Verify<ITvRequestEngine>(x => x.RequestTvShow(It.Is<TvRequestViewModelV2>(x => x.TheMovieDbId == 123)), Times.Once);
            _mocker.Verify<IPlexApi>(x => x.GetWatchlistMetadata("abc", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _mocker.Verify<ITvRequestEngine>(x => x.SetUser(It.Is<OmbiUser>(x => x.Id == "abc")), Times.Once);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.GetAll(), Times.Once);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.Add(It.IsAny<PlexWatchlistHistory>()), Times.Never);
        }

        [Test]
        public async Task MovieRequestFromWatchList_NoTmdbGuid()
        {
            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings { Enable = true, EnableWatchlistImport = true });
            _mocker.Setup<IPlexApi, Task<PlexWatchlistContainer>>(x => x.GetWatchlist(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PlexWatchlistContainer
            {
                MediaContainer = new PlexWatchlist
                {
                    Metadata = new List<Metadata>
                    {
                        new Metadata
                        {
                            type = "movie",
                            ratingKey = "abc"
                        }
                    }
                }
            });
            _mocker.Setup<IPlexApi, Task<PlexWatchlistMetadataContainer>>(x => x.GetWatchlistMetadata("abc", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlexWatchlistMetadataContainer
                {
                    MediaContainer = new PlexWatchlistMetadata
                    {
                        Metadata = new WatchlistMetadata[]
                        {
                            new WatchlistMetadata
                            {
                                Guid = new List<PlexGuids>
                                {
                                    new PlexGuids
                                    {
                                        Id = "imdb://123"
                                    }
                                }
                            }
                        }

                    }
                });
            _mocker.Setup<IMovieRequestEngine, Task<RequestEngineResult>>(x => x.RequestMovie(It.IsAny<MovieRequestViewModel>()))
                .ReturnsAsync(new RequestEngineResult { RequestId = 1 });
            await _subject.Execute(_context.Object);
            _mocker.Verify<IMovieRequestEngine>(x => x.RequestMovie(It.IsAny<MovieRequestViewModel>()), Times.Never);
            _mocker.Verify<IPlexApi>(x => x.GetWatchlistMetadata("abc", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _mocker.Verify<IMovieRequestEngine>(x => x.SetUser(It.Is<OmbiUser>(x => x.Id == "abc")), Times.Never);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.Add(It.IsAny<PlexWatchlistHistory>()), Times.Never);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.GetAll(), Times.Never);
        }

        [Test]
        public async Task TvRequestFromWatchList_NoTmdbGuid()
        {
            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings { Enable = true, EnableWatchlistImport = true });
            _mocker.Setup<IPlexApi, Task<PlexWatchlistContainer>>(x => x.GetWatchlist(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PlexWatchlistContainer
            {
                MediaContainer = new PlexWatchlist
                {
                    Metadata = new List<Metadata>
                    {
                        new Metadata
                        {
                            type = "movie",
                            ratingKey = "abc"
                        }
                    }
                }
            });
            _mocker.Setup<IPlexApi, Task<PlexWatchlistMetadataContainer>>(x => x.GetWatchlistMetadata("abc", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlexWatchlistMetadataContainer
                {
                    MediaContainer = new PlexWatchlistMetadata
                    {
                        Metadata = new WatchlistMetadata[]
                        {
                            new WatchlistMetadata
                            {
                                Guid = new List<PlexGuids>
                                {
                                    new PlexGuids
                                    {
                                        Id = "imdb://123"
                                    }
                                }
                            }
                        }

                    }
                });
            _mocker.Setup<ITvRequestEngine, Task<RequestEngineResult>>(x => x.RequestTvShow(It.IsAny<TvRequestViewModelV2>()))
                .ReturnsAsync(new RequestEngineResult { RequestId = 1 });
            await _subject.Execute(_context.Object);
            _mocker.Verify<ITvRequestEngine>(x => x.RequestTvShow(It.IsAny<TvRequestViewModelV2>()), Times.Never);
            _mocker.Verify<IPlexApi>(x => x.GetWatchlistMetadata("abc", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _mocker.Verify<ITvRequestEngine>(x => x.SetUser(It.Is<OmbiUser>(x => x.Id == "abc")), Times.Never);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.Add(It.IsAny<PlexWatchlistHistory>()), Times.Never);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.GetAll(), Times.Never);
        }

        [Test]
        public async Task MovieRequestFromWatchList_AlreadyImported()
        {
            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings { Enable = true, EnableWatchlistImport = true });
            _mocker.Setup<IPlexApi, Task<PlexWatchlistContainer>>(x => x.GetWatchlist(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PlexWatchlistContainer
            {
                MediaContainer = new PlexWatchlist
                {
                    Metadata = new List<Metadata>
                    {
                        new Metadata
                        {
                            type = "movie",
                            ratingKey = "abc"
                        }
                    }
                }
            });
            _mocker.Setup<IPlexApi, Task<PlexWatchlistMetadataContainer>>(x => x.GetWatchlistMetadata("abc", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlexWatchlistMetadataContainer
                {
                    MediaContainer = new PlexWatchlistMetadata
                    {
                        Metadata = new WatchlistMetadata[]
                        {
                            new WatchlistMetadata
                            {
                                Guid = new List<PlexGuids>
                                {
                                    new PlexGuids
                                    {
                                        Id = "tmdb://123"
                                    }
                                }
                            }
                        }

                    }
                });
            _mocker.Setup<IExternalRepository<PlexWatchlistHistory>, IQueryable<PlexWatchlistHistory>>(x => x.GetAll()).Returns(new List<PlexWatchlistHistory> { new PlexWatchlistHistory { Id = 1, TmdbId = "123" } }.AsQueryable());
            await _subject.Execute(_context.Object);
            _mocker.Verify<IMovieRequestEngine>(x => x.RequestMovie(It.IsAny<MovieRequestViewModel>()), Times.Never);
            _mocker.Verify<IPlexApi>(x => x.GetWatchlistMetadata("abc", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _mocker.Verify<IMovieRequestEngine>(x => x.SetUser(It.Is<OmbiUser>(x => x.Id == "abc")), Times.Never);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.Add(It.IsAny<PlexWatchlistHistory>()), Times.Never);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.GetAll(), Times.Once);
        }
    }
}
