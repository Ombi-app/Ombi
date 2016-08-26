#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: PlexAvailabilityCheckerTests.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.Plex;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Services.Interfaces;
using PlexRequests.Helpers;
using PlexRequests.Services.Jobs;
using PlexRequests.Services.Models;
using PlexRequests.Store.Models;
using PlexRequests.Store.Repository;

using Ploeh.AutoFixture;

namespace PlexRequests.Services.Tests
{
    [TestFixture]
    public class PlexAvailabilityCheckerTests
    {
        public IAvailabilityChecker Checker { get; set; }
        private Fixture F { get; set; } = new Fixture();
        private Mock<ISettingsService<PlexSettings>> SettingsMock { get; set; }
        private Mock<ISettingsService<AuthenticationSettings>> AuthMock { get; set; }
        private Mock<IRequestService> RequestMock { get; set; }
        private Mock<IPlexApi> PlexMock { get; set; }
        private Mock<ICacheProvider> CacheMock { get; set; }
        private Mock<INotificationService> NotificationMock { get; set; }
        private Mock<IJobRecord> JobRec { get; set; }
        private Mock<IRepository<UsersToNotify>> NotifyUsers { get; set; }
        private Mock<IRepository<PlexEpisodes>> PlexEpisodes { get; set; }

        [SetUp]
        public void Setup()
        {
            SettingsMock = new Mock<ISettingsService<PlexSettings>>();
            AuthMock = new Mock<ISettingsService<AuthenticationSettings>>();
            RequestMock = new Mock<IRequestService>();
            PlexMock = new Mock<IPlexApi>();
            NotificationMock = new Mock<INotificationService>();
            CacheMock = new Mock<ICacheProvider>();
            NotifyUsers = new Mock<IRepository<UsersToNotify>>();
            PlexEpisodes = new Mock<IRepository<PlexEpisodes>>();
            JobRec = new Mock<IJobRecord>();
            Checker = new PlexAvailabilityChecker(SettingsMock.Object, RequestMock.Object, PlexMock.Object, CacheMock.Object, NotificationMock.Object, JobRec.Object, NotifyUsers.Object, PlexEpisodes.Object);

        }

        [Test]
        public void InvalidSettings()
        {
            Checker.CheckAndUpdateAll();
            PlexMock.Verify(x => x.GetLibrary(It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<string>()), Times.Never);
            PlexMock.Verify(x => x.GetAccount(It.IsAny<string>()), Times.Never);
            PlexMock.Verify(x => x.GetLibrarySections(It.IsAny<string>(), It.IsAny<Uri>()), Times.Never);
            PlexMock.Verify(x => x.GetStatus(It.IsAny<string>(), It.IsAny<Uri>()), Times.Never);
            PlexMock.Verify(x => x.GetUsers(It.IsAny<string>()), Times.Never);
        }

        [TestCaseSource(nameof(IsMovieAvailableTestData))]
        public bool IsMovieAvailableTest(string title, string year)
        {
            var movies = new List<PlexMovie>
            {
                new PlexMovie {Title = title, ProviderId = null, ReleaseYear = year}
            };
            var result = Checker.IsMovieAvailable(movies.ToArray(), "title", "2011");

            return result;
        }

        private static IEnumerable<TestCaseData> IsMovieAvailableTestData
        {
            get
            {
                yield return new TestCaseData("title", "2011").Returns(true).SetName("IsMovieAvailable True");
                yield return new TestCaseData("title2", "2011").Returns(false).SetName("IsMovieAvailable False different title");
                yield return new TestCaseData("title", "2001").Returns(false).SetName("IsMovieAvailable False different year");
            }
        }


        [TestCaseSource(nameof(IsMovieAvailableAdvancedTestData))]
        public bool IsMovieAvailableAdvancedTest(string title, string year, string providerId)
        {
            var movies = new List<PlexMovie>
            {
                new PlexMovie {Title = title, ProviderId = providerId, ReleaseYear = year }
            };
            var result = Checker.IsMovieAvailable(movies.ToArray(), "title", "2011", 9999.ToString());

            return result;
        }

        private static IEnumerable<TestCaseData> IsMovieAvailableAdvancedTestData
        {
            get
            {
                yield return new TestCaseData("title", "2011", "9999").Returns(true).SetName("Advanced IsMovieAvailable True");
                yield return new TestCaseData("title2", "2011", "99929").Returns(false).SetName("Advanced IsMovieAvailable False different title");
                yield return new TestCaseData("title", "2001", "99939").Returns(false).SetName("Advanced IsMovieAvailable False different year");
                yield return new TestCaseData("title", "2001", "44445").Returns(false).SetName("Advanced IsMovieAvailable False different providerID");
            }
        }

        [TestCaseSource(nameof(IsTvAvailableTestData))]
        public bool IsTvAvailableTest(string title, string year)
        {
            var tv = new List<PlexTvShow>
            {
                new PlexTvShow {Title = title, ProviderId = null, ReleaseYear = year}
            };
            var result = Checker.IsTvShowAvailable(tv.ToArray(), "title", "2011");

            return result;
        }

        private static IEnumerable<TestCaseData> IsTvAvailableTestData
        {
            get
            {
                yield return new TestCaseData("title", "2011").Returns(true).SetName("IsTvAvailable True");
                yield return new TestCaseData("title2", "2011").Returns(false).SetName("IsTvAvailable False different title");
                yield return new TestCaseData("title", "2001").Returns(false).SetName("IsTvAvailable False different year");
            }
        }

        [TestCaseSource(nameof(IsTvAvailableAdvancedTestData))]
        public bool IsTvAvailableAdvancedTest(string title, string year, string providerId)
        {
            var movies = new List<PlexTvShow>
            {
                new PlexTvShow {Title = title, ProviderId = providerId, ReleaseYear = year }
            };
            var result = Checker.IsTvShowAvailable(movies.ToArray(), "title", "2011", 9999.ToString());

            return result;
        }

        private static IEnumerable<TestCaseData> IsTvAvailableAdvancedTestData
        {
            get
            {
                yield return new TestCaseData("title", "2011", "9999").Returns(true).SetName("Advanced IsTvAvailable True");
                yield return new TestCaseData("title2", "2011", "99929").Returns(false).SetName("Advanced IsTvAvailable False different title");
                yield return new TestCaseData("title", "2001", "99939").Returns(false).SetName("Advanced IsTvAvailable False different year");
                yield return new TestCaseData("title", "2001", "44445").Returns(false).SetName("Advanced IsTvAvailable False different providerID");
            }
        }

        [TestCaseSource(nameof(IsTvAvailableAdvancedSeasonsTestData))]
        public bool IsTvAvailableAdvancedSeasonsTest(string title, string year, string providerId, int[] seasons)
        {
            var movies = new List<PlexTvShow>
            {
                new PlexTvShow {Title = title, ProviderId = providerId, ReleaseYear = year , Seasons = seasons}
            };
            var result = Checker.IsTvShowAvailable(movies.ToArray(), "title", "2011", 9999.ToString(), new[] { 1, 2, 3 });

            return result;
        }

        private static IEnumerable<TestCaseData> IsTvAvailableAdvancedSeasonsTestData
        {
            get
            {
                yield return new TestCaseData("title", "2011", "9999", new[] { 1, 2, 3 }).Returns(true).SetName("Advanced IsTvSeasonsAvailable True");
                yield return new TestCaseData("title2", "2011", "99929", new[] { 5, 6 }).Returns(false).SetName("Advanced IsTvSeasonsAvailable False no seasons");
                yield return new TestCaseData("title2", "2011", "9999", new[] { 1, 6 }).Returns(true).SetName("Advanced IsTvSeasonsAvailable true one season");
            }
        }

        [TestCaseSource(nameof(IsEpisodeAvailableTestData))]
        public bool IsEpisodeAvailableTest(string providerId, int season, int episode)
        {
            var expected = new List<PlexEpisodes>
            {
                new PlexEpisodes {EpisodeNumber = 1, ShowTitle = "The Flash",ProviderId = 23.ToString(), SeasonNumber = 1, EpisodeTitle = "Pilot"}
            };
            PlexEpisodes.Setup(x => x.Custom(It.IsAny<Func<IDbConnection, IEnumerable<PlexEpisodes>>>())).Returns(expected);
            Checker = new PlexAvailabilityChecker(SettingsMock.Object, RequestMock.Object, PlexMock.Object, CacheMock.Object, NotificationMock.Object, JobRec.Object, NotifyUsers.Object, PlexEpisodes.Object);

            var result = Checker.IsEpisodeAvailable(providerId, season, episode);

            return result;
        }

        private static IEnumerable<TestCaseData> IsEpisodeAvailableTestData
        {
            get
            {
                yield return new TestCaseData("23", 1, 1).Returns(true).SetName("IsEpisodeAvailable True S01E01");
                yield return new TestCaseData("23", 1, 2).Returns(false).SetName("IsEpisodeAvailable False S01E02");
                yield return new TestCaseData("23", 99, 99).Returns(false).SetName("IsEpisodeAvailable False S99E99");
                yield return new TestCaseData("230", 99, 99).Returns(false).SetName("IsEpisodeAvailable False Incorrect ProviderId");
            }
        }

        [Test]
        public void GetPlexMoviesTests()
        {
            var cachedMovies = F.Build<PlexSearch>().Without(x => x.Directory).CreateMany().ToList();
            cachedMovies.Add(new PlexSearch
            {
                Video = new List<Video>
                {
                    new Video {Type = "movie", Title = "title1", Year = "2016", ProviderId = "1212"}
                }
            });
            CacheMock.Setup(x => x.Get<List<PlexSearch>>(CacheKeys.PlexLibaries)).Returns(cachedMovies);
            SettingsMock.Setup(x => x.GetSettings()).Returns(F.Create<PlexSettings>());
            var movies = Checker.GetPlexMovies();

            Assert.That(movies.Any(x => x.ProviderId == "1212"));
        }

        [Test]
        public void GetPlexTvShowsTests()
        {
            var cachedTv = F.Build<PlexSearch>().Without(x => x.Directory).CreateMany().ToList();
            cachedTv.Add(new PlexSearch
            {
                Directory = new List<Directory1>
                {
                    new Directory1 {Type = "show", Title = "title1", Year = "2016", ProviderId = "1212", Seasons = new List<Directory1>()}
                }
            });
            SettingsMock.Setup(x => x.GetSettings()).Returns(F.Create<PlexSettings>());
            CacheMock.Setup(x => x.Get<List<PlexSearch>>(CacheKeys.PlexLibaries)).Returns(cachedTv);
            var movies = Checker.GetPlexTvShows();

            Assert.That(movies.Any(x => x.ProviderId == "1212"));
        }

        [Test]
        public async Task GetAllPlexEpisodes()
        {
            PlexEpisodes.Setup(x => x.GetAllAsync()).ReturnsAsync(F.CreateMany<PlexEpisodes>().ToList());
            Checker = new PlexAvailabilityChecker(SettingsMock.Object, RequestMock.Object, PlexMock.Object, CacheMock.Object, NotificationMock.Object, JobRec.Object, NotifyUsers.Object, PlexEpisodes.Object);

            var episodes = await Checker.GetEpisodes();

            Assert.That(episodes.Count(), Is.GreaterThan(0));
        }
    }
}