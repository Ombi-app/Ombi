using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.External.MediaServers.Plex;
using Ombi.Api.External.MediaServers.Plex.Models;
using Ombi.Api.External.MediaServers.Plex.Models.Community;
using Ombi.Api.External.MediaServers.Plex.Models.Friends;
using Ombi.Core.Authentication;
using Ombi.Core.Engine;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models;
using Ombi.Core.Models.Requests;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
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
        private const string AdminOmbiId = "admin-id";

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            var users = new List<OmbiUser>
            {
                new OmbiUser { Id = AdminOmbiId, UserName = "owner", NormalizedUserName = "OWNER", UserType = UserType.PlexUser, ProviderUserId = AdminUuid, MediaServerToken = AdminToken },
            };
            var userMgr = MockHelper.MockUserManager(users);
            SetupAdminRole(userMgr, AdminOmbiId);
            _mocker.Use(userMgr);
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

        private void UseDefaultPlexSettings(bool enable = true, bool watchlist = true, bool monitorAll = false)
        {
            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new PlexSettings
                {
                    Enable = enable,
                    EnableWatchlistImport = watchlist,
                    MonitorAll = monitorAll,
                    Servers = new List<PlexServers>
                    {
                        new PlexServers { Name = "test", MachineIdentifier = "m1", PlexAuthToken = AdminToken }
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
        public async Task TerminatesWhenAdminHasNoOAuthToken()
        {
            // Server exists, but no admin Plex user has a MediaServerToken (i.e. the owner
            // has not signed into Ombi via Plex OAuth). The import must not call plex.tv.
            var users = new List<OmbiUser>
            {
                new OmbiUser { Id = AdminOmbiId, UserName = "owner", NormalizedUserName = "OWNER", UserType = UserType.PlexUser, ProviderUserId = AdminUuid, MediaServerToken = null },
            };
            var userMgr = MockHelper.MockUserManager(users);
            SetupAdminRole(userMgr, AdminOmbiId);
            _mocker.Use(userMgr);
            _subject = _mocker.CreateInstance<PlexWatchlistImport>();
            UseDefaultPlexSettings();

            await _subject.Execute(_context.Object);

            _mocker.Verify<IPlexApi>(x => x.GetAllFriends(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _mocker.Verify<IPlexApi>(x => x.GetAccount(It.IsAny<string>()), Times.Never);
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
            SetupWatchlistNode(AdminUuid, "movie", "rk-1");
            SetupMetadataWithTmdb("rk-1", "tmdb://42");

            _mocker.Setup<IMovieRequestEngine, Task<RequestEngineResult>>(x => x.RequestMovie(It.IsAny<MovieRequestViewModel>()))
                .ReturnsAsync(new RequestEngineResult { Result = true, Message = "ok" });

            await _subject.Execute(_context.Object);

            _mocker.Verify<IMovieRequestEngine>(x => x.SetUser(It.Is<OmbiUser>(u => u.ProviderUserId == AdminUuid)), Times.Once);
            _mocker.Verify<IMovieRequestEngine>(x => x.RequestMovie(It.Is<MovieRequestViewModel>(m => m.TheMovieDbId == 42)), Times.Once);
            _mocker.Verify<IExternalRepository<PlexWatchlistHistory>>(x => x.Add(It.Is<PlexWatchlistHistory>(h => h.TmdbId == "42")), Times.Once);
            _statusStore.Verify(x => x.SetAsync(AdminOmbiId, WatchlistSyncStatus.Successful, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GraphqlErrorForUser_MarksFailed_NotSuccessful()
        {
            UseDefaultPlexSettings();
            _mocker.Setup<IPlexApi, Task<PlexCommunityWatchlistResponse>>(x => x.GetWatchlistForUser(AdminToken, AdminUuid, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlexCommunityWatchlistResponse
                {
                    errors = new List<PlexCommunityError> { new PlexCommunityError { message = "denied" } }
                });

            await _subject.Execute(_context.Object);

            _statusStore.Verify(x => x.SetAsync(AdminOmbiId, WatchlistSyncStatus.Failed, It.IsAny<CancellationToken>()), Times.Once);
            _statusStore.Verify(x => x.SetAsync(AdminOmbiId, WatchlistSyncStatus.Successful, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ShowInWatchlist_RequestsLatestSeason_WhenMonitorAllFalse()
        {
            UseDefaultPlexSettings();
            SetupWatchlistNode(AdminUuid, "show", "rk-2");
            SetupMetadataWithTmdb("rk-2", "tmdb://77");

            _mocker.Setup<ITvRequestEngine, Task<RequestEngineResult>>(x => x.RequestTvShow(It.IsAny<TvRequestViewModelV2>()))
                .ReturnsAsync(new RequestEngineResult { Result = true });

            await _subject.Execute(_context.Object);

            _mocker.Verify<ITvRequestEngine>(x => x.RequestTvShow(It.Is<TvRequestViewModelV2>(t => t.TheMovieDbId == 77 && t.LatestSeason && !t.RequestAll)), Times.Once);
        }

        [Test]
        public async Task ShowInWatchlist_RequestsAll_WhenMonitorAllTrue()
        {
            UseDefaultPlexSettings(monitorAll: true);
            SetupWatchlistNode(AdminUuid, "show", "rk-3");
            SetupMetadataWithTmdb("rk-3", "tmdb://99");

            _mocker.Setup<ITvRequestEngine, Task<RequestEngineResult>>(x => x.RequestTvShow(It.IsAny<TvRequestViewModelV2>()))
                .ReturnsAsync(new RequestEngineResult { Result = true });

            await _subject.Execute(_context.Object);

            _mocker.Verify<ITvRequestEngine>(x => x.RequestTvShow(It.Is<TvRequestViewModelV2>(t => t.TheMovieDbId == 99 && t.RequestAll && !t.LatestSeason)), Times.Once);
        }

        [Test]
        public async Task CaseInsensitiveNodeType_IsAccepted()
        {
            UseDefaultPlexSettings();
            SetupWatchlistNode(AdminUuid, "MOVIE", "rk-case");
            SetupMetadataWithTmdb("rk-case", "tmdb://1");

            _mocker.Setup<IMovieRequestEngine, Task<RequestEngineResult>>(x => x.RequestMovie(It.IsAny<MovieRequestViewModel>()))
                .ReturnsAsync(new RequestEngineResult { Result = true });

            await _subject.Execute(_context.Object);

            _mocker.Verify<IMovieRequestEngine>(x => x.RequestMovie(It.Is<MovieRequestViewModel>(m => m.TheMovieDbId == 1)), Times.Once);
        }

        [Test]
        public async Task NonNumericTmdbId_IsSkipped()
        {
            UseDefaultPlexSettings();
            SetupWatchlistNode(AdminUuid, "movie", "rk-bad");
            SetupMetadataWithTmdb("rk-bad", "tmdb://not-a-number");

            await _subject.Execute(_context.Object);

            _mocker.Verify<IMovieRequestEngine>(x => x.RequestMovie(It.IsAny<MovieRequestViewModel>()), Times.Never);
        }

        [Test]
        public async Task FriendInAllFriendsV2_IsProcessed()
        {
            UseDefaultPlexSettings();
            SetupFriends(("friend-uuid", "friend"));
            SetupLegacyUsers(("12345", "friend"));

            await _subject.Execute(_context.Object);

            _mocker.Verify<IPlexApi>(x => x.GetWatchlistForUser(AdminToken, "friend-uuid", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task FriendFailure_MarkedAsFailed_OnNewlyCreatedOmbiUser()
        {
            UseDefaultPlexSettings();
            SetupFriends(("friend-uuid", "some-friend"));
            SetupLegacyUsers(("54321", "some-friend"));
            _mocker.Setup<IPlexApi, Task<PlexCommunityWatchlistResponse>>(x => x.GetWatchlistForUser(AdminToken, "friend-uuid", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlexCommunityWatchlistResponse
                {
                    errors = new List<PlexCommunityError> { new PlexCommunityError { message = "denied" } }
                });

            await _subject.Execute(_context.Object);

            _statusStore.Verify(x => x.SetAsync(It.Is<string>(id => !string.IsNullOrEmpty(id) && id != AdminOmbiId), WatchlistSyncStatus.Failed, It.IsAny<CancellationToken>()), Times.Once);
            _statusStore.Verify(x => x.SetAsync(AdminOmbiId, WatchlistSyncStatus.Failed, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task CommunityOnlyFriendNotInLegacyUsersList_IsSkipped()
        {
            // /api/users only contains friends with server-share access. A community-only
            // friend isn't there, so we have no numeric plex.tv id and no way to make their
            // row participate in PlexUserImporter cleanup, /Token/plextoken auth, etc.
            // Auto-creating them would lead to PlexUserImporter.CleanupPlexUsers deleting
            // them on its next run. Skip them entirely; ask the operator to share the server
            // with the friend if they want their watchlist synced.
            UseDefaultPlexSettings();
            SetupFriends(("brand-new", "newbie"));
            // Default _plexApi.GetUsers mock returns null — friend not in legacy list.

            await _subject.Execute(_context.Object);

            _mocker.Verify<Core.Authentication.OmbiUserManager>(x => x.CreateAsync(It.IsAny<OmbiUser>()), Times.Never);
            _mocker.Verify<IPlexApi>(x => x.GetWatchlistForUser(AdminToken, "brand-new", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task NewFriendInLegacyUsersList_IsCreatedWithNumericPlexId()
        {
            // The community API returns a UUID for friends, but the rest of Ombi
            // (PlexUserImporter, GetOmbiUserFromPlexToken, the wizard) keys on the numeric
            // plex.tv id. When /api/users gives us the numeric id for this username we use
            // it, so the new row stays consistent with everything else.
            UseDefaultPlexSettings();
            SetupFriends(("community-uuid", "newbie"));
            SetupLegacyUsers(("99887766", "newbie"));

            await _subject.Execute(_context.Object);

            _mocker.Verify<Core.Authentication.OmbiUserManager>(x => x.CreateAsync(It.Is<OmbiUser>(u => u.UserName == "newbie" && u.ProviderUserId == "99887766" && u.UserType == UserType.PlexUser)), Times.Once);
            // The watchlist call still uses the community UUID — that's the API's id space.
            _mocker.Verify<IPlexApi>(x => x.GetWatchlistForUser(AdminToken, "community-uuid", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task AdminWithoutPriorOmbiRow_IsCreatedWithNumericPlexId()
        {
            // GetAccount returns both the UUID and the numeric id for the admin. The new
            // admin row should be created with the numeric id so /Token/plextoken works
            // for them on first login.
            var users = new List<OmbiUser>
            {
                // Some other admin sourced the OAuth token, but the plex.tv account being
                // resolved doesn't yet have an Ombi row by username.
                new OmbiUser { Id = AdminOmbiId, UserName = "tokenholder", NormalizedUserName = "TOKENHOLDER", UserType = UserType.PlexUser, ProviderUserId = "tokenholder-uuid", MediaServerToken = AdminToken },
            };
            var userMgr = MockHelper.MockUserManager(users);
            SetupAdminRole(userMgr, AdminOmbiId);
            _mocker.Use(userMgr);
            _mocker.Setup<IPlexApi, Task<PlexAccount>>(x => x.GetAccount(AdminToken))
                .ReturnsAsync(new PlexAccount { user = new User { uuid = "owner-uuid", id = "11223344", username = "owner-account", title = "Owner Account" } });
            _subject = _mocker.CreateInstance<PlexWatchlistImport>();
            UseDefaultPlexSettings();

            await _subject.Execute(_context.Object);

            _mocker.Verify<Core.Authentication.OmbiUserManager>(x => x.CreateAsync(It.Is<OmbiUser>(u => u.UserName == "owner-account" && u.ProviderUserId == "11223344" && u.UserType == UserType.PlexUser)), Times.Once);
        }

        [Test]
        public async Task ExistingFriendWhoRenamedPlexAccount_IsAdoptedNotDuplicated()
        {
            // A Plex user previously imported with ProviderUserId = "12345" and UserName
            // = "oldname" later changes their plex.tv username to "newname". The community
            // API now reports them with the new username, and /api/users still maps newname
            // -> "12345". Without resolving the numeric id up-front and matching on either
            // the UUID or the numeric id, we'd miss the existing row (no UUID match, no
            // username match) and create a second row pointing at the same "12345".
            const string numericId = "12345";
            var users = new List<OmbiUser>
            {
                new OmbiUser { Id = AdminOmbiId, UserName = "owner", NormalizedUserName = "OWNER", UserType = UserType.PlexUser, ProviderUserId = AdminUuid, MediaServerToken = AdminToken },
                new OmbiUser { Id = "renamed-id", UserName = "oldname", NormalizedUserName = "OLDNAME", UserType = UserType.PlexUser, ProviderUserId = numericId },
            };
            var userMgr = MockHelper.MockUserManager(users);
            SetupAdminRole(userMgr, AdminOmbiId);
            _mocker.Use(userMgr);
            _subject = _mocker.CreateInstance<PlexWatchlistImport>();
            UseDefaultPlexSettings();
            SetupFriends(("renamed-uuid", "newname"));
            SetupLegacyUsers((numericId, "newname"));

            await _subject.Execute(_context.Object);

            _mocker.Verify<Core.Authentication.OmbiUserManager>(x => x.CreateAsync(It.IsAny<OmbiUser>()), Times.Never);
            _mocker.Verify<IPlexApi>(x => x.GetWatchlistForUser(AdminToken, "renamed-uuid", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _statusStore.Verify(x => x.SetAsync("renamed-id", WatchlistSyncStatus.Successful, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ExistingFriendWithCommunityUuidProviderId_IsAdoptedViaUsernameFallback()
        {
            // Regression for the 4.59 -> 4.60 upgrade path. Ombi 4.59's watchlist importer
            // auto-created friends with the community UUID in ProviderUserId. After this PR
            // we never store UUIDs again, but those rows are still in users' databases.
            // The username fallback must adopt them (the stored UUID equals the target's id,
            // so the collision check passes), so the existing row keeps being used and we
            // don't produce a duplicate via the create path.
            const string communityUuid = "old-friend-uuid";
            var users = new List<OmbiUser>
            {
                new OmbiUser { Id = AdminOmbiId, UserName = "owner", NormalizedUserName = "OWNER", UserType = UserType.PlexUser, ProviderUserId = AdminUuid, MediaServerToken = AdminToken },
                new OmbiUser { Id = "uuid-row-id", UserName = "uuidfriend", NormalizedUserName = "UUIDFRIEND", UserType = UserType.PlexUser, ProviderUserId = communityUuid },
            };
            var userMgr = MockHelper.MockUserManager(users);
            SetupAdminRole(userMgr, AdminOmbiId);
            _mocker.Use(userMgr);
            _subject = _mocker.CreateInstance<PlexWatchlistImport>();
            UseDefaultPlexSettings();
            SetupFriends((communityUuid, "uuidfriend"));
            SetupLegacyUsers(("44556677", "uuidfriend"));

            await _subject.Execute(_context.Object);

            _mocker.Verify<Core.Authentication.OmbiUserManager>(x => x.CreateAsync(It.IsAny<OmbiUser>()), Times.Never);
            _mocker.Verify<Core.Authentication.OmbiUserManager>(x => x.UpdateAsync(It.Is<OmbiUser>(u => u.Id == "uuid-row-id")), Times.Never);
            _mocker.Verify<IPlexApi>(x => x.GetWatchlistForUser(AdminToken, communityUuid, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _statusStore.Verify(x => x.SetAsync("uuid-row-id", WatchlistSyncStatus.Successful, It.IsAny<CancellationToken>()), Times.Once);
            // The 4.59-era UUID stays put — we don't rewrite it.
            Assert.That(users.Single(u => u.Id == "uuid-row-id").ProviderUserId, Is.EqualTo(communityUuid));
        }

        [Test]
        public async Task ExistingFriendWithLegacyNumericProviderId_IsAdoptedWithoutRewritingProviderUserId()
        {
            // Regression for https://github.com/Ombi-app/Ombi/issues/5399.
            // User was previously imported via the legacy /api/users path, which stores the
            // numeric plex.tv account id in ProviderUserId. The community API returns a UUID
            // for the same user. The import must:
            //   * adopt the existing row by username (no duplicate, no skip),
            //   * leave ProviderUserId untouched so /Token/plextoken (which matches on the
            //     numeric id) keeps working,
            //   * sync the watchlist against the community UUID,
            //   * not mark the user as NotAFriend in the post-run sweep.
            const string legacyNumericId = "12345678";
            const string communityUuid = "friend-uuid";
            var users = new List<OmbiUser>
            {
                new OmbiUser { Id = AdminOmbiId, UserName = "owner", NormalizedUserName = "OWNER", UserType = UserType.PlexUser, ProviderUserId = AdminUuid, MediaServerToken = AdminToken },
                new OmbiUser { Id = "legacy-id", UserName = "legacyfriend", NormalizedUserName = "LEGACYFRIEND", UserType = UserType.PlexUser, ProviderUserId = legacyNumericId },
            };
            var userMgr = MockHelper.MockUserManager(users);
            SetupAdminRole(userMgr, AdminOmbiId);
            _mocker.Use(userMgr);
            _subject = _mocker.CreateInstance<PlexWatchlistImport>();
            UseDefaultPlexSettings();
            SetupFriends((communityUuid, "legacyfriend"));

            await _subject.Execute(_context.Object);

            _mocker.Verify<Core.Authentication.OmbiUserManager>(x => x.CreateAsync(It.IsAny<OmbiUser>()), Times.Never);
            // Crucially: do not rewrite ProviderUserId on the legacy row — that would break
            // /api/v1/Token/plextoken which still resolves users by numeric plex.tv id.
            _mocker.Verify<Core.Authentication.OmbiUserManager>(x => x.UpdateAsync(It.Is<OmbiUser>(u => u.Id == "legacy-id")), Times.Never);
            Assert.That(users.Single(u => u.Id == "legacy-id").ProviderUserId, Is.EqualTo(legacyNumericId));

            // Watchlist syncs against the community UUID, not the stored numeric id.
            _mocker.Verify<IPlexApi>(x => x.GetWatchlistForUser(AdminToken, communityUuid, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _statusStore.Verify(x => x.SetAsync("legacy-id", WatchlistSyncStatus.Successful, It.IsAny<CancellationToken>()), Times.Once);
            // And the post-run sweep must not mislabel a user we just synced.
            _statusStore.Verify(x => x.SetAsync("legacy-id", WatchlistSyncStatus.NotAFriend, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ExistingFriendUsernameCollisionWithDifferentUuid_IsSkipped()
        {
            // A row already bound to a non-numeric (community-style) ProviderUserId that
            // doesn't match the incoming community id is a real collision (e.g. a Plex
            // username was reused after the original account was deleted). We must not
            // hijack the existing row, and we must not sync against the new uuid.
            // The sweep should ALSO flip the existing row to NotAFriend — it's a stale
            // ghost: the original Plex account it was bound to is no longer a friend.
            var users = new List<OmbiUser>
            {
                new OmbiUser { Id = AdminOmbiId, UserName = "owner", NormalizedUserName = "OWNER", UserType = UserType.PlexUser, ProviderUserId = AdminUuid, MediaServerToken = AdminToken },
                new OmbiUser { Id = "other-id", UserName = "duplicate", NormalizedUserName = "DUPLICATE", UserType = UserType.PlexUser, ProviderUserId = "real-uuid-for-other" },
            };
            var userMgr = MockHelper.MockUserManager(users);
            SetupAdminRole(userMgr, AdminOmbiId);
            _mocker.Use(userMgr);
            _subject = _mocker.CreateInstance<PlexWatchlistImport>();
            UseDefaultPlexSettings();
            SetupFriends(("different-uuid", "duplicate"));

            await _subject.Execute(_context.Object);

            _mocker.Verify<Core.Authentication.OmbiUserManager>(x => x.UpdateAsync(It.IsAny<OmbiUser>()), Times.Never);
            _mocker.Verify<IPlexApi>(x => x.GetWatchlistForUser(AdminToken, "different-uuid", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _statusStore.Verify(x => x.SetAsync("other-id", WatchlistSyncStatus.NotAFriend, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task BannedFriendWithExistingOmbiRow_IsNotMarkedNotAFriend()
        {
            // EnsureOmbiUser returns null for banned ids, so the row never lands in
            // matchedUserIds. The sweep must still recognise the user as a current friend
            // via the target list and leave the row alone — banned ≠ unfriended.
            const string bannedUuid = "banned-uuid";
            var users = new List<OmbiUser>
            {
                new OmbiUser { Id = AdminOmbiId, UserName = "owner", NormalizedUserName = "OWNER", UserType = UserType.PlexUser, ProviderUserId = AdminUuid, MediaServerToken = AdminToken },
                new OmbiUser { Id = "banned-id", UserName = "bannedfriend", NormalizedUserName = "BANNEDFRIEND", UserType = UserType.PlexUser, ProviderUserId = bannedUuid },
            };
            var userMgr = MockHelper.MockUserManager(users);
            SetupAdminRole(userMgr, AdminOmbiId);
            _mocker.Use(userMgr);
            _mocker.Setup<ISettingsService<UserManagementSettings>, Task<UserManagementSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { BannedPlexUserIds = new List<string> { bannedUuid } });
            _subject = _mocker.CreateInstance<PlexWatchlistImport>();
            UseDefaultPlexSettings();
            SetupFriends((bannedUuid, "bannedfriend"));

            await _subject.Execute(_context.Object);

            _statusStore.Verify(x => x.SetAsync("banned-id", WatchlistSyncStatus.NotAFriend, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task CancelledRun_DoesNotSweepNotAFriend()
        {
            // If the run is cancelled mid-loop, matchedUserIds may be incomplete. The sweep
            // must not run, otherwise valid friends that weren't yet processed get mislabelled.
            var users = new List<OmbiUser>
            {
                new OmbiUser { Id = AdminOmbiId, UserName = "owner", NormalizedUserName = "OWNER", UserType = UserType.PlexUser, ProviderUserId = AdminUuid, MediaServerToken = AdminToken },
                new OmbiUser { Id = "friend-id", UserName = "friend", NormalizedUserName = "FRIEND", UserType = UserType.PlexUser, ProviderUserId = "friend-uuid" },
            };
            var userMgr = MockHelper.MockUserManager(users);
            SetupAdminRole(userMgr, AdminOmbiId);
            _mocker.Use(userMgr);
            _subject = _mocker.CreateInstance<PlexWatchlistImport>();
            UseDefaultPlexSettings();
            SetupFriends(("friend-uuid", "friend"));

            using var cts = new CancellationTokenSource();
            cts.Cancel();
            _context.Setup(x => x.CancellationToken).Returns(cts.Token);

            await _subject.Execute(_context.Object);

            _statusStore.Verify(x => x.SetAsync(It.IsAny<string>(), WatchlistSyncStatus.NotAFriend, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task PreExistingPlexUser_NotInAllFriendsV2_IsMarkedNotAFriend()
        {
            // Seed an additional Plex user that no target will match (their ProviderUserId
            // isn't in allFriendsV2 and isn't the admin).
            var extraUsers = new List<OmbiUser>
            {
                new OmbiUser { Id = AdminOmbiId, UserName = "owner", NormalizedUserName = "OWNER", UserType = UserType.PlexUser, ProviderUserId = AdminUuid, MediaServerToken = AdminToken },
                new OmbiUser { Id = "orphan-id", UserName = "orphan", NormalizedUserName = "ORPHAN", UserType = UserType.PlexUser, ProviderUserId = "orphan-uuid" },
            };
            var userMgr = MockHelper.MockUserManager(extraUsers);
            SetupAdminRole(userMgr, AdminOmbiId);
            _mocker.Use(userMgr);
            _subject = _mocker.CreateInstance<PlexWatchlistImport>();
            UseDefaultPlexSettings();

            await _subject.Execute(_context.Object);

            _statusStore.Verify(x => x.SetAsync("orphan-id", WatchlistSyncStatus.NotAFriend, It.IsAny<CancellationToken>()), Times.Once);
            _statusStore.Verify(x => x.SetAsync(AdminOmbiId, WatchlistSyncStatus.NotAFriend, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ExceptionDuringImport_MarkedFailed()
        {
            UseDefaultPlexSettings();
            _mocker.Setup<IPlexApi, Task<PlexCommunityWatchlistResponse>>(x => x.GetWatchlistForUser(AdminToken, AdminUuid, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new System.InvalidOperationException("boom"));

            await _subject.Execute(_context.Object);

            _statusStore.Verify(x => x.SetAsync(AdminOmbiId, WatchlistSyncStatus.Failed, It.IsAny<CancellationToken>()), Times.Once);
            _statusStore.Verify(x => x.SetAsync(AdminOmbiId, WatchlistSyncStatus.Successful, It.IsAny<CancellationToken>()), Times.Never);
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

        [Test]
        public async Task BannedByLegacyNumericId_IsSkipped()
        {
            // PlexUserImporter stores the numeric plex.tv id in BannedPlexUserIds when an admin
            // bans a user. The watchlist importer sees UUIDs from the community API, so the ban
            // check has to accept either id shape or numeric bans silently let the user through.
            const string numericId = "555444";
            _mocker.Setup<ISettingsService<UserManagementSettings>, Task<UserManagementSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new UserManagementSettings { BannedPlexUserIds = new List<string> { numericId } });
            UseDefaultPlexSettings();
            SetupFriends(("banned-uuid", "bannedbyname"));
            SetupLegacyUsers((numericId, "bannedbyname"));

            await _subject.Execute(_context.Object);

            _mocker.Verify<IPlexApi>(x => x.GetWatchlistForUser(AdminToken, "banned-uuid", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _mocker.Verify<Core.Authentication.OmbiUserManager>(x => x.CreateAsync(It.IsAny<OmbiUser>()), Times.Never);
        }

        private static void SetupAdminRole(Mock<OmbiUserManager> mgr, string adminUserId)
        {
            mgr.Setup(x => x.IsInRoleAsync(It.Is<OmbiUser>(u => u.Id == adminUserId), OmbiRoles.Admin))
                .ReturnsAsync(true);
            mgr.Setup(x => x.IsInRoleAsync(It.Is<OmbiUser>(u => u.Id != adminUserId), OmbiRoles.Admin))
                .ReturnsAsync(false);
        }

        private void SetupLegacyUsers(params (string id, string username)[] users)
        {
            _mocker.Setup<IPlexApi, Task<PlexUsers>>(x => x.GetUsers(AdminToken))
                .ReturnsAsync(new PlexUsers
                {
                    User = users.Select(u => new UserFriends { Id = u.id, Username = u.username }).ToArray()
                });
        }

        private void SetupFriends(params (string id, string username)[] friends)
        {
            _mocker.Setup<IPlexApi, Task<PlexCommunityFriendsResponse>>(x => x.GetAllFriends(AdminToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlexCommunityFriendsResponse
                {
                    data = new PlexCommunityFriendsData
                    {
                        allFriendsV2 = friends.Select(f => new PlexCommunityFriend
                        {
                            user = new PlexCommunityUser { id = f.id, username = f.username }
                        }).ToList()
                    }
                });
        }

        private void SetupWatchlistNode(string ownerId, string type, string ratingKey)
        {
            _mocker.Setup<IPlexApi, Task<PlexCommunityWatchlistResponse>>(x => x.GetWatchlistForUser(AdminToken, ownerId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
