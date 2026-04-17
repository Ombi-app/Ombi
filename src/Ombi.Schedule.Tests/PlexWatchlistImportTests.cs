using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.External.MediaServers.Plex;
using Ombi.Api.External.MediaServers.Plex.Models;
using Ombi.Api.External.MediaServers.Plex.Models.Community;
using Ombi.Core.Engine;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models;
using Ombi.Core.Models.Requests;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Schedule.Jobs.Plex;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Test.Common;
using Quartz;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserType = Ombi.Store.Entities.UserType;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class PlexWatchlistImportTests
    {
        private PlexWatchlistImport _subject;
        private AutoMocker _mocker;
        private Mock<IJobExecutionContext> _context;
        private Mock<IPlexWatchlistStatusStore> _statusStore;

        private const string AdminToken = "admin-token";
        private const string AdminUuid = "admin-uuid";

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            var users = new List<OmbiUser>
            {
                new OmbiUser { Id = "admin-id", UserName = "owner", NormalizedUserName = "OWNER", UserType = UserType.PlexUser, ProviderUserId = AdminUuid },
            };
            _mocker.Use(MockHelper.MockUserManager(users));
            _context = _mocker.GetMock<IJobExecutionContext>();
            _context.Setup(x => x.CancellationToken).Returns(CancellationToken.None);
            _statusStore = _mocker.GetMock<IPlexWatchlistStatusStore>();

            _mocker.Setup<IExternalRepository<PlexWatchlistHistory>, IQueryable<PlexWatchlistHistory>>(x => x.GetAll())
                .Returns(new List<PlexWatchlistHistory>().AsQueryable().BuildMock());

            _mocker.Setup<ISettingsService<UserManagementSettings>, Task<UserManagementSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new UserManagementSettings());

            _mocker.Setup<IPlexApi, Task<PlexAccount>>(x => x.GetAccount(AdminToken))
                .ReturnsAsync(new PlexAccount { user = new User { uuid = AdminUuid, username = "owner", title = "Owner" } });

            _mocker.Setup<IPlexApi, Task<PlexCommunityFriendsResponse>>(x => x.GetAllFriends(AdminToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlexCommunityFriendsResponse
                {
                    data = new PlexCommunityFriendsData { allFriendsV2 = new List<PlexCommunityFriend>() }
                });

            _mocker.Setup<IPlexApi, Task<PlexCommunityWatchlistResponse>>(x => x.GetWatchlistForUser(AdminToken, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlexCommunityWatchlistResponse
                {
                    data = new PlexCommunityWatchlistData { userV2 = new PlexCommunityUserV2 { watchlist = new PlexCommunityWatchlist { nodes = new List<PlexCommunityWatchlistNode>(), pageInfo = new PlexCommunityPageInfo() } } }
                });

            _subject = _mocker.CreateInstance<PlexWatchlistImport>();
        }

        private void UseDefaultPlexSettings(bool enable = true, bool watchlist = true)
        {
            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new PlexSettings
                {
                    Enable = enable,
                    EnableWatchlistImport = watchlist,
                    Servers = new List<PlexServers>
                    {
                        new PlexServers { PlexAuthToken = AdminToken }
                    },
                });
        }

        [Test]
        public async Task TerminatesWhenPlexIsNotEnabled()
        {
            UseDefaultPlexSettings(enable: false);
            await _subject.Execute(_context.Object);
            _mocker.Verify<IPlexApi>(x => x.GetAllFriends(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task TerminatesWhenWatchlistIsNotEnabled()
        {
            UseDefaultPlexSettings(watchlist: false);
            await _subject.Execute(_context.Object);
            _mocker.Verify<IPlexApi>(x => x.GetAllFriends(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task TerminatesWhenNoAdminTokenConfigured()
        {
            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new PlexSettings { Enable = true, EnableWatchlistImport = true, Servers = new List<PlexServers>() });

            await _subject.Execute(_context.Object);

            _mocker.Verify<IPlexApi>(x => x.GetAllFriends(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task FetchesFriendsListWithAdminToken()
        {
            UseDefaultPlexSettings();
            await _subject.Execute(_context.Object);

            _mocker.Verify<IPlexApi>(x => x.GetAllFriends(AdminToken, It.IsAny<CancellationToken>()), Times.Once);
            _mocker.Verify<IPlexApi>(x => x.GetWatchlistForUser(AdminToken, AdminUuid, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task EmptyFriendList_ProcessesOnlyAdminWatchlist()
        {
            UseDefaultPlexSettings();
            await _subject.Execute(_context.Object);

            _mocker.Verify<IPlexApi>(x => x.GetWatchlistForUser(AdminToken, AdminUuid, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _mocker.Verify<IPlexApi>(x => x.GetWatchlistForUser(AdminToken, It.Is<string>(id => id != AdminUuid), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task MovieInWatchlist_RequestsMovieAndAddsHistory()
        {
            UseDefaultPlexSettings();
            SetupWatchlistNode("movie", "rk-1");
            SetupMetadataWithTmdb("rk-1", "tmdb://42");

            _mocker.Setup<IMovieRequestEngine, Task<RequestEngineResult>>(x => x.RequestMovie(It.IsAny<MovieRequestViewModel>()))
                .ReturnsAsync(new RequestEngineResult { Result = true, Message = "ok" });

            await _subject.Execute(_context.Object);

            _mocker.Verify<IMovieRequestEngine>(x => x.SetUser(It.Is<OmbiUser>(u => u.ProviderUserId == AdminUuid)), Times.Once);
            _mocker.Verify<IMovieRequestEngine>(x => x.RequestMovie(It.Is<MovieRequestViewModel>(m => m.TheMovieDbId == 42)), Times.Once);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.Add(It.Is<PlexWatchlistHistory>(h => h.TmdbId == "42")), Times.Once);
            _statusStore.Verify(x => x.Set("admin-id", WatchlistSyncStatus.Successful), Times.Once);
        }

        [Test]
        public async Task ShowInWatchlist_RequestsLatestSeason_WhenMonitorAllFalse()
        {
            UseDefaultPlexSettings();
            SetupWatchlistNode("show", "rk-2");
            SetupMetadataWithTmdb("rk-2", "tmdb://77");

            _mocker.Setup<ITvRequestEngine, Task<RequestEngineResult>>(x => x.RequestTvShow(It.IsAny<TvRequestViewModelV2>()))
                .ReturnsAsync(new RequestEngineResult { Result = true });

            await _subject.Execute(_context.Object);

            _mocker.Verify<ITvRequestEngine>(x => x.RequestTvShow(It.Is<TvRequestViewModelV2>(t => t.TheMovieDbId == 77 && t.LatestSeason && !t.RequestAll)), Times.Once);
        }

        [Test]
        public async Task ShowInWatchlist_RequestsAll_WhenMonitorAllTrue()
        {
            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new PlexSettings
                {
                    Enable = true,
                    EnableWatchlistImport = true,
                    MonitorAll = true,
                    Servers = new List<PlexServers> { new PlexServers { PlexAuthToken = AdminToken } },
                });
            SetupWatchlistNode("show", "rk-3");
            SetupMetadataWithTmdb("rk-3", "tmdb://99");

            _mocker.Setup<ITvRequestEngine, Task<RequestEngineResult>>(x => x.RequestTvShow(It.IsAny<TvRequestViewModelV2>()))
                .ReturnsAsync(new RequestEngineResult { Result = true });

            await _subject.Execute(_context.Object);

            _mocker.Verify<ITvRequestEngine>(x => x.RequestTvShow(It.Is<TvRequestViewModelV2>(t => t.TheMovieDbId == 99 && t.RequestAll && !t.LatestSeason)), Times.Once);
        }

        [Test]
        public async Task FriendInAllFriendsV2_IsProcessed()
        {
            UseDefaultPlexSettings();
            _mocker.Setup<IPlexApi, Task<PlexCommunityFriendsResponse>>(x => x.GetAllFriends(AdminToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlexCommunityFriendsResponse
                {
                    data = new PlexCommunityFriendsData
                    {
                        allFriendsV2 = new List<PlexCommunityFriend>
                        {
                            new PlexCommunityFriend { user = new PlexCommunityUser { id = "friend-uuid", username = "friend" } }
                        }
                    }
                });

            await _subject.Execute(_context.Object);

            _mocker.Verify<IPlexApi>(x => x.GetWatchlistForUser(AdminToken, "friend-uuid", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task FriendFailure_MarkedAsFailed()
        {
            UseDefaultPlexSettings();
            _mocker.Setup<IPlexApi, Task<PlexCommunityFriendsResponse>>(x => x.GetAllFriends(AdminToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlexCommunityFriendsResponse
                {
                    data = new PlexCommunityFriendsData
                    {
                        allFriendsV2 = new List<PlexCommunityFriend>
                        {
                            new PlexCommunityFriend { user = new PlexCommunityUser { id = "friend-uuid", username = "some-friend" } }
                        }
                    }
                });
            _mocker.Setup<IPlexApi, Task<PlexCommunityWatchlistResponse>>(x => x.GetWatchlistForUser(AdminToken, "friend-uuid", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlexCommunityWatchlistResponse
                {
                    errors = new List<PlexCommunityError> { new PlexCommunityError { message = "denied" } }
                });

            await _subject.Execute(_context.Object);

            _statusStore.Verify(x => x.Set(It.IsAny<string>(), WatchlistSyncStatus.Failed), Times.Once);
        }

        [Test]
        public async Task NewFriendWithoutOmbiUser_IsCreated()
        {
            UseDefaultPlexSettings();
            _mocker.Setup<IPlexApi, Task<PlexCommunityFriendsResponse>>(x => x.GetAllFriends(AdminToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlexCommunityFriendsResponse
                {
                    data = new PlexCommunityFriendsData
                    {
                        allFriendsV2 = new List<PlexCommunityFriend>
                        {
                            new PlexCommunityFriend { user = new PlexCommunityUser { id = "brand-new", username = "newbie" } }
                        }
                    }
                });

            await _subject.Execute(_context.Object);

            _mocker.Verify<Core.Authentication.OmbiUserManager>(x => x.CreateAsync(It.Is<OmbiUser>(u => u.UserName == "newbie" && u.ProviderUserId == "brand-new" && u.UserType == UserType.PlexUser)), Times.Once);
        }

        [Test]
        public async Task BannedPlexUser_IsSkipped()
        {
            _mocker.Setup<ISettingsService<UserManagementSettings>, Task<UserManagementSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { BannedPlexUserIds = new List<string> { AdminUuid } });
            UseDefaultPlexSettings();

            await _subject.Execute(_context.Object);

            _mocker.Verify<IPlexApi>(x => x.GetWatchlistForUser(AdminToken, AdminUuid, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private void SetupWatchlistNode(string type, string ratingKey)
        {
            _mocker.Setup<IPlexApi, Task<PlexCommunityWatchlistResponse>>(x => x.GetWatchlistForUser(AdminToken, AdminUuid, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlexCommunityWatchlistResponse
                {
                    data = new PlexCommunityWatchlistData
                    {
                        userV2 = new PlexCommunityUserV2
                        {
                            watchlist = new PlexCommunityWatchlist
                            {
                                nodes = new List<PlexCommunityWatchlistNode> { new PlexCommunityWatchlistNode { id = ratingKey, title = "Test", type = type } },
                                pageInfo = new PlexCommunityPageInfo { hasNextPage = false },
                            }
                        }
                    }
                });
        }

        private void SetupMetadataWithTmdb(string ratingKey, string tmdbGuid)
        {
            _mocker.Setup<IPlexApi, Task<PlexWatchlistMetadataContainer>>(x => x.GetWatchlistMetadata(ratingKey, AdminToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlexWatchlistMetadataContainer
                {
                    MediaContainer = new PlexWatchlistMetadata
                    {
                        Metadata = new[]
                        {
                            new WatchlistMetadata
                            {
                                Guid = new List<PlexGuids> { new PlexGuids { Id = tmdbGuid } }
                            }
                        }
                    }
                });
        }
    }
}
