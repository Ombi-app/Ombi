using AutoFixture;
using NUnit.Framework;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;
using System.Collections.Generic;
using System.Linq;

namespace Ombi.Notifications.Tests
{
    public class NotificationMessageCurlysTests
    {
        private NotificationMessageCurlys sut { get; set; }
        private Fixture F { get; set; }

        [SetUp]
        public void Setup()
        {
            F = new Fixture();
            F.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => F.Behaviors.Remove(b));
            F.Behaviors.Add(new OmitOnRecursionBehavior());
            sut = new NotificationMessageCurlys();
        }

        [Test]
        public void MovieNotificationTests()
        {
            var notificationOptions = new NotificationOptions();
            var req = F.Build<MovieRequests>()
                .With(x => x.RequestType, RequestType.Movie)
                .With(x => x.Available, true)
                .Create();
            var customization = new CustomizationSettings
            {
                ApplicationUrl = "url",
                ApplicationName = "name"
            };
            var userPrefs = new UserNotificationPreferences();
            sut.Setup(notificationOptions, req, customization, userPrefs);

            Assert.That(req.Id.ToString(), Is.EqualTo(sut.RequestId));
            Assert.That(req.TheMovieDbId.ToString(), Is.EqualTo(sut.ProviderId));
            Assert.That(req.Title.ToString(), Is.EqualTo(sut.Title));
            Assert.That(req.RequestedUser.UserName, Is.EqualTo(sut.RequestedUser));
            Assert.That(req.RequestedUser.Alias, Is.EqualTo(sut.Alias));
            Assert.That(req.RequestedDate.ToString("D"), Is.EqualTo(sut.RequestedDate));
            Assert.That("Movie", Is.EqualTo(sut.Type));
            Assert.That(req.Overview, Is.EqualTo(sut.Overview));
            Assert.That(req.ReleaseDate.Year.ToString(), Is.EqualTo(sut.Year));
            Assert.That(req.DeniedReason, Is.EqualTo(sut.DenyReason));
            Assert.That(req.MarkedAsAvailable?.ToString("D"), Is.EqualTo(sut.AvailableDate));
            Assert.That("https://image.tmdb.org/t/p/w300/" + req.PosterPath, Is.EqualTo(sut.PosterImage));
            Assert.That(req.DeniedReason, Is.EqualTo(sut.DenyReason));
            Assert.That(req.RequestedUser.Alias, Is.EqualTo(sut.UserPreference));
            Assert.That(string.Empty, Is.EqualTo(sut.AdditionalInformation));
            Assert.That("Available", Is.EqualTo(sut.RequestStatus));
            Assert.That("url", Is.EqualTo(sut.ApplicationUrl));
            Assert.That("name", Is.EqualTo(sut.ApplicationName));
        }

        [Test]
        public void MovieIssueNotificationTests()
        {
            var notificationOptions = new NotificationOptions
            {
                Substitutes = new Dictionary<string, string>
                {
                    { "IssueDescription", "Desc" },
                    { "IssueCategory", "Cat" },
                    { "IssueStatus", "state" },
                    { "IssueSubject", "sub" },
                    { "NewIssueComment", "a" },
                    { "IssueUser", "User" },
                    { "IssueUserAlias", "alias" },
                    { "RequestType", "Movie" },
                }
            };
            var req = F.Build<MovieRequests>()
                .With(x => x.RequestType, RequestType.Movie)
                .Create();
            var customization = new CustomizationSettings();
            var userPrefs = new UserNotificationPreferences();
            sut.Setup(notificationOptions, req, customization, userPrefs);

            Assert.That("Desc", Is.EqualTo(sut.IssueDescription));
            Assert.That("Cat", Is.EqualTo(sut.IssueCategory));
            Assert.That("state", Is.EqualTo(sut.IssueStatus));
            Assert.That("a", Is.EqualTo(sut.NewIssueComment));
            Assert.That("User", Is.EqualTo(sut.UserName));
            Assert.That("alias", Is.EqualTo(sut.Alias));
            Assert.That("Movie", Is.EqualTo(sut.Type));
        }

        [Test]
        public void MovieNotificationUserPreferences()
        {
            var notificationOptions = new NotificationOptions
            {
                AdditionalInformation = "add"
            };
            var req = F.Build<MovieRequests>()
                .With(x => x.RequestType, RequestType.Movie)
                .Without(x => x.MarkedAsAvailable)
                .Create();
            var customization = new CustomizationSettings();
            var userPrefs = new UserNotificationPreferences
            {
                Value = "PrefValue"
            };
            sut.Setup(notificationOptions, req, customization, userPrefs);

            Assert.That("PrefValue", Is.EqualTo(sut.UserPreference));
            Assert.That(string.Empty, Is.EqualTo(sut.AvailableDate));
            Assert.That("add", Is.EqualTo(sut.AdditionalInformation));
        }

        [TestCaseSource(nameof(RequestStatusData))]
        public string MovieNotificationTests_RequestStatus(bool available, bool denied, bool approved)
        {
            var notificationOptions = new NotificationOptions();
            var req = F.Build<MovieRequests>()
                .With(x => x.RequestType, RequestType.Movie)
                .With(x => x.Available, available)
                .With(x => x.Denied, denied)
                .With(x => x.Approved, approved)
                .Create();
            var customization = new CustomizationSettings();
            var userPrefs = new UserNotificationPreferences();
            sut.Setup(notificationOptions, req, customization, userPrefs);
            return sut.RequestStatus;
        }

        private static IEnumerable<TestCaseData> RequestStatusData
        {
            get
            {
                yield return new TestCaseData(true, false, false).Returns("Available");
                yield return new TestCaseData(false, true, false).Returns("Denied");
                yield return new TestCaseData(false, false, true).Returns("Processing Request");
                yield return new TestCaseData(false, false, false).Returns("Pending Approval");
            }
        }


        [Test]
        public void NewsletterTests()
        {
            var customization = new CustomizationSettings
            {
                ApplicationUrl = "url",
                ApplicationName = "name"
            };
            sut.SetupNewsletter(customization);

            Assert.That("url", Is.EqualTo(sut.ApplicationUrl));
            Assert.That("name", Is.EqualTo(sut.ApplicationName));
        }

        [Test]
        public void MusicNotificationTests()
        {
            var notificationOptions = new NotificationOptions();
            var req = F.Build<MusicRequests>()
                .With(x => x.RequestType, RequestType.Album)
                .With(x => x.Available, true)
                .Create();
            var customization = new CustomizationSettings
            {
                ApplicationUrl = "url",
                ApplicationName = "name"
            };
            var userPrefs = new UserNotificationPreferences();
            sut.Setup(notificationOptions, req, customization, userPrefs);

            Assert.That(req.Id.ToString(), Is.EqualTo(sut.RequestId));
            Assert.That(req.ForeignArtistId.ToString(), Is.EqualTo(sut.ProviderId));
            Assert.That(req.Title.ToString(), Is.EqualTo(sut.Title));
            Assert.That(req.RequestedUser.UserName, Is.EqualTo(sut.RequestedUser));
            Assert.That(req.RequestedUser.Alias, Is.EqualTo(sut.Alias));
            Assert.That(req.RequestedDate.ToString("D"), Is.EqualTo(sut.RequestedDate));
            Assert.That("Album", Is.EqualTo(sut.Type));
            Assert.That(req.ReleaseDate.Year.ToString(), Is.EqualTo(sut.Year));
            Assert.That(req.DeniedReason, Is.EqualTo(sut.DenyReason));
            Assert.That(req.MarkedAsAvailable?.ToString("D"), Is.EqualTo(sut.AvailableDate));
            Assert.That(req.Cover, Is.EqualTo(sut.PosterImage));
            Assert.That(req.DeniedReason, Is.EqualTo(sut.DenyReason));
            Assert.That(req.RequestedUser.Alias, Is.EqualTo(sut.UserPreference));
            Assert.That(string.Empty, Is.EqualTo(sut.AdditionalInformation));
            Assert.That("Available", Is.EqualTo(sut.RequestStatus));
            Assert.That("url", Is.EqualTo(sut.ApplicationUrl));
            Assert.That("name", Is.EqualTo(sut.ApplicationName));
        }

        [TestCaseSource(nameof(RequestStatusData))]
        public string MusicNotificationTests_RequestStatus(bool available, bool denied, bool approved)
        {
            var notificationOptions = new NotificationOptions();
            var req = F.Build<MusicRequests>()
                .With(x => x.RequestType, RequestType.Album)
                .With(x => x.Available, available)
                .With(x => x.Denied, denied)
                .With(x => x.Approved, approved)
                .Create();
            var customization = new CustomizationSettings();
            var userPrefs = new UserNotificationPreferences();
            sut.Setup(notificationOptions, req, customization, userPrefs);
            return sut.RequestStatus;
        }

        [Test]
        public void TvNotificationTests()
        {
            var notificationOptions = new NotificationOptions();
            var req = F.Build<ChildRequests>()
                .With(x => x.RequestType, RequestType.TvShow)
                .With(x => x.Available, true)
                .Create();
            var customization = new CustomizationSettings
            {
                ApplicationUrl = "url",
                ApplicationName = "name"
            };
            var userPrefs = new UserNotificationPreferences();
            sut.Setup(notificationOptions, req, customization, userPrefs);

            Assert.That(req.Id.ToString(), Is.EqualTo(sut.RequestId));
            Assert.That(req.ParentRequest.ExternalProviderId.ToString(), Is.EqualTo(sut.ProviderId));
            Assert.That(req.ParentRequest.Title.ToString(), Is.EqualTo(sut.Title));
            Assert.That(req.RequestedUser.UserName, Is.EqualTo(sut.RequestedUser));
            Assert.That(req.RequestedUser.Alias, Is.EqualTo(sut.Alias));
            Assert.That(req.RequestedDate.ToString("D"), Is.EqualTo(sut.RequestedDate));
            Assert.That("TV Show", Is.EqualTo(sut.Type));
            Assert.That(req.ParentRequest.Overview, Is.EqualTo(sut.Overview));
            Assert.That(req.ParentRequest.ReleaseDate.Year.ToString(), Is.EqualTo(sut.Year));
            Assert.That(req.DeniedReason, Is.EqualTo(sut.DenyReason));
            Assert.That(req.MarkedAsAvailable?.ToString("D"), Is.EqualTo(sut.AvailableDate));
            Assert.That("https://image.tmdb.org/t/p/w300/" + req.ParentRequest.PosterPath, Is.EqualTo(sut.PosterImage));
            Assert.That(req.DeniedReason, Is.EqualTo(sut.DenyReason));
            Assert.That(req.RequestedUser.Alias, Is.EqualTo(sut.UserPreference));
            Assert.That(null, Is.EqualTo(sut.AdditionalInformation));
            Assert.That("Available", Is.EqualTo(sut.RequestStatus));
            Assert.That("url", Is.EqualTo(sut.ApplicationUrl));
            Assert.That("name", Is.EqualTo(sut.ApplicationName));
        }

        [Test]
        public void TvNotification_EpisodeList()
        {
            var episodeRequests = new List<EpisodeRequests>
            {
                new EpisodeRequests
                {
                    EpisodeNumber = 1,
                },
                new EpisodeRequests
                {
                    EpisodeNumber = 2,
                },
                new EpisodeRequests
                {
                    EpisodeNumber = 3,
                }
            };
            var seasonRequests = new List<SeasonRequests>
            {
                new SeasonRequests
                {
                    Episodes = episodeRequests,
                    SeasonNumber = 1
                },
                new SeasonRequests
                {
                    Episodes = episodeRequests,
                    SeasonNumber = 2
                },
                new SeasonRequests
                {
                    Episodes = episodeRequests,
                    SeasonNumber = 3
                }
            };


            var notificationOptions = new NotificationOptions();
            var req = F.Build<ChildRequests>()
                .With(x => x.RequestType, RequestType.TvShow)
                .With(x => x.Available, true)
                .With(x => x.SeasonRequests, seasonRequests)
                .Create();
            var customization = new CustomizationSettings
            {
                ApplicationUrl = "url",
                ApplicationName = "name"
            };
            var userPrefs = new UserNotificationPreferences();
            sut.Setup(notificationOptions, req, customization, userPrefs);

            Assert.That(sut.EpisodesList, Is.EqualTo("1,1,1,2,2,2,3,3,3"));
            Assert.That(sut.SeasonsList, Is.EqualTo("1,2,3"));

        }

        [TestCaseSource(nameof(RequestStatusData))]
        public string TvShowNotificationTests_RequestStatus(bool available, bool denied, bool approved)
        {
            var notificationOptions = new NotificationOptions();
            var req = F.Build<ChildRequests>()
                .With(x => x.RequestType, RequestType.TvShow)
                .With(x => x.Available, available)
                .With(x => x.Denied, denied)
                .With(x => x.Approved, approved)
                .Create();
            var customization = new CustomizationSettings();
            var userPrefs = new UserNotificationPreferences();
            sut.Setup(notificationOptions, req, customization, userPrefs);
            return sut.RequestStatus;
        }


        [Test]
        public void EmailSetupTests()
        {
            var user = F.Create<OmbiUser>();
            var customization = new CustomizationSettings
            {
                ApplicationUrl = "url",
                ApplicationName = "name"
            };
            sut.Setup(user, customization);

            Assert.That(user.UserName, Is.EqualTo(sut.RequestedUser));
            Assert.That(user.UserName, Is.EqualTo(sut.UserName));
            Assert.That(user.UserAlias, Is.EqualTo(sut.Alias));
            Assert.That(sut.ApplicationUrl, Is.EqualTo("url"));
            Assert.That(sut.ApplicationName, Is.EqualTo("name"));
        }

    }
}
