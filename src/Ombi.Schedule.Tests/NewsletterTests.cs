using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.External;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Test.Common;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class NewsletterTests
    {
        [Test]
        public async Task GetMoviesContent_DeduplicatesSameMediaAcrossDifferentRows()
        {
            var result = await RunGetMoviesContent(new[]
            {
                new PlexServerContent
                {
                    Id = 1,
                    Type = MediaType.Movie,
                    TheMovieDbId = "123",
                    ImdbId = "tt123",
                    Title = "Movie A",
                    AddedAt = DateTime.UtcNow,
                    Key = "movie-1"
                },
                new PlexServerContent
                {
                    Id = 2,
                    Type = MediaType.Movie,
                    TheMovieDbId = "123",
                    ImdbId = "tt123",
                    Title = "Movie A Alternate Release",
                    AddedAt = DateTime.UtcNow.AddMinutes(1),
                    Key = "movie-2"
                }
            }, "123");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Single().TheMovieDbId, Is.EqualTo("123"));
        }

        [Test]
        public async Task GetMoviesContent_DeduplicatesByImdbId_WhenTheMovieDbIdMissing()
        {
            var result = await RunGetMoviesContent(new[]
            {
                new PlexServerContent
                {
                    Id = 1,
                    Type = MediaType.Movie,
                    TheMovieDbId = " ",
                    ImdbId = " TT123 ",
                    Title = "Movie A",
                    AddedAt = DateTime.UtcNow,
                    Key = "movie-1"
                },
                new PlexServerContent
                {
                    Id = 2,
                    Type = MediaType.Movie,
                    TheMovieDbId = " ",
                    ImdbId = "tt123",
                    Title = "Movie A Alternate Release",
                    AddedAt = DateTime.UtcNow.AddMinutes(1),
                    Key = "movie-2"
                }
            }, string.Empty);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Single().ImdbId.Trim(), Is.EqualTo("TT123").IgnoreCase);
        }

        [Test]
        public async Task GetMoviesContent_KeepsDistinctRows_WhenNoExternalIdsPresent()
        {
            var result = await RunGetMoviesContent(new[]
            {
                new PlexServerContent
                {
                    Id = 1,
                    Type = MediaType.Movie,
                    TheMovieDbId = " ",
                    ImdbId = " ",
                    Title = "Movie A",
                    AddedAt = DateTime.UtcNow,
                    Key = "movie-1"
                },
                new PlexServerContent
                {
                    Id = 2,
                    Type = MediaType.Movie,
                    TheMovieDbId = " ",
                    ImdbId = " ",
                    Title = "Movie B",
                    AddedAt = DateTime.UtcNow.AddMinutes(1),
                    Key = "movie-2"
                }
            }, string.Empty);

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [TestCaseSource(nameof(EpisodeListData))]
        public string BuildEpisodeListTest(List<int> episodes)
        {
            var ep = new List<int>();
            foreach (var i in episodes)
            {
                ep.Add(i);
            }
            var result = StringHelper.BuildEpisodeList(ep);
            return result;
        }

        public static IEnumerable<TestCaseData> EpisodeListData
        {
            get
            {
                yield return new TestCaseData(new List<int> { 1, 2, 3, 4, 5, 6 }).Returns("1-6").SetName("Simple 1-6");
                yield return new TestCaseData(new List<int> { 1, 2, 3, 4, 5, 6, 8, 9 }).Returns("1-6, 8-9").SetName("Simple 1-6, 8-9");
                yield return new TestCaseData(new List<int> { 1, 99, 101, 555, 468, 469 }).Returns("1, 99, 101, 555, 468-469").SetName("More Complex");
                yield return new TestCaseData(new List<int> { 1 }).Returns("1").SetName("Single Episode");
            }
        }

        private sealed class TestExternalContext : ExternalContext
        {
            public TestExternalContext(DbContextOptions<TestExternalContext> options) : base(options)
            {
            }
        }

        private sealed class TestOmbiContext : OmbiContext
        {
            public TestOmbiContext(DbContextOptions<TestOmbiContext> options) : base(options)
            {
            }
        }

        private static async Task<HashSet<IMediaServerContent>> RunGetMoviesContent(params PlexServerContent[] items)
        {
            return await RunGetMoviesContent(items, "123");
        }

        private static async Task<HashSet<IMediaServerContent>> RunGetMoviesContent(IEnumerable<PlexServerContent> items, string refreshMovieDbId)
        {
            var externalOptions = new DbContextOptionsBuilder<TestExternalContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var ombiOptions = new DbContextOptionsBuilder<TestOmbiContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var externalContext = new TestExternalContext(externalOptions);
            await using var ombiContext = new TestOmbiContext(ombiOptions);

            externalContext.PlexServerContent.AddRange(items);
            await externalContext.SaveChangesAsync();

            var plexRepository = new PlexServerContentRepository(externalContext);
            var recentlyAddedRepository = new Repository<RecentlyAddedLog>(ombiContext);

            var mocker = new AutoMocker();
            mocker.Use<IPlexContentRepository>(plexRepository);
            mocker.Use<IRepository<RecentlyAddedLog>>(recentlyAddedRepository);
            mocker.Use<Microsoft.AspNetCore.Identity.UserManager<OmbiUser>>(MockHelper.MockUserManager(new List<OmbiUser>()).Object);

            var embyRepository = new Mock<IEmbyContentRepository>();
            embyRepository.Setup(x => x.GetAll()).Returns(new List<EmbyContent>().AsQueryable());
            embyRepository.Setup(x => x.GetAllEpisodes()).Returns(new List<IMediaServerEpisode>().AsQueryable());
            mocker.Use<IEmbyContentRepository>(embyRepository.Object);

            var jellyfinRepository = new Mock<IJellyfinContentRepository>();
            jellyfinRepository.Setup(x => x.GetAll()).Returns(new List<JellyfinContent>().AsQueryable());
            jellyfinRepository.Setup(x => x.GetAllEpisodes()).Returns(new List<IMediaServerEpisode>().AsQueryable());
            mocker.Use<IJellyfinContentRepository>(jellyfinRepository.Object);

            mocker.GetMock<ISettingsService<EmailNotificationSettings>>()
                .Setup(x => x.GetSettingsAsync()).ReturnsAsync(new EmailNotificationSettings());
            mocker.GetMock<ISettingsService<NewsletterSettings>>()
                .Setup(x => x.GetSettingsAsync()).ReturnsAsync(new NewsletterSettings());
            mocker.GetMock<ISettingsService<CustomizationSettings>>()
                .Setup(x => x.GetSettingsAsync()).ReturnsAsync(new CustomizationSettings());
            mocker.GetMock<ISettingsService<LidarrSettings>>()
                .Setup(x => x.GetSettingsAsync()).ReturnsAsync(new LidarrSettings());
            mocker.GetMock<ISettingsService<OmbiSettings>>()
                .Setup(x => x.GetSettingsAsync()).ReturnsAsync(new OmbiSettings());
            mocker.GetMock<ISettingsService<PlexSettings>>()
                .Setup(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings { Enable = true });
            mocker.GetMock<ISettingsService<EmbySettings>>()
                .Setup(x => x.GetSettingsAsync()).ReturnsAsync(new EmbySettings());
            mocker.GetMock<ISettingsService<JellyfinSettings>>()
                .Setup(x => x.GetSettingsAsync()).ReturnsAsync(new JellyfinSettings());
            mocker.GetMock<IRefreshMetadata>()
                .Setup(x => x.GetTheMovieDbId(false, true, null, It.IsAny<string>(), It.IsAny<string>(), true))
                .ReturnsAsync(refreshMovieDbId);

            var subject = mocker.CreateInstance<NewsletterJob>();

            var method = typeof(NewsletterJob).GetMethod("GetMoviesContent", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null);

            var generic = method.MakeGenericMethod(typeof(PlexServerContent));
            var task = (Task<HashSet<IMediaServerContent>>)generic.Invoke(subject, new object[] { plexRepository, false });
            return await task;
        }
    }
}
