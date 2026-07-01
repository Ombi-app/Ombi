using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.External.MediaServers.Emby;
using Ombi.Api.External.MediaServers.Emby.Models;
using Ombi.Api.External.MediaServers.Emby.Models.Media.Tv;
using Ombi.Api.External.MediaServers.Emby.Models.Movie;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Hubs;
using Ombi.Schedule.Jobs.Emby;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Test.Common;
using Quartz;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class EmbyPlayedSyncTests
    {
        private AutoMocker _mocker;
        private EmbyPlayedSync _subject;
        private Mock<IEmbyApi> _api;
        private Mock<IJobExecutionContext> _context;
        private List<UserPlayedEpisode> _addedEpisodes;
        private List<UserPlayedMovie> _addedMovies;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();

            _api = new Mock<IEmbyApi>();
            _mocker.Setup<IEmbyApiFactory, IEmbyApi>(x => x.CreateClient(It.IsAny<EmbySettings>()))
                .Returns(_api.Object);

            _mocker.Setup<ISettingsService<EmbySettings>, Task<EmbySettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new EmbySettings
                {
                    Enable = true,
                    Servers = new List<EmbyServers>
                    {
                        new EmbyServers
                        {
                            Name = "Test",
                            ApiKey = "key",
                            AdministratorId = "admin",
                            Ip = "localhost",
                            Port = 8096
                        }
                    }
                });

            var users = new List<OmbiUser>
            {
                new OmbiUser { Id = "user1", UserName = "embyUser", UserType = UserType.EmbyUser, ProviderUserId = "embyUser1" }
            };
            _mocker.Use(MockHelper.MockUserManager(users));

            // The sync passes live collections that it clears afterwards, so snapshot
            // their contents at call time rather than inspecting them at verify time
            _addedEpisodes = new List<UserPlayedEpisode>();
            _mocker.GetMock<IUserPlayedEpisodeRepository>()
                .Setup(x => x.AddRange(It.IsAny<IEnumerable<UserPlayedEpisode>>(), It.IsAny<bool>()))
                .Callback<IEnumerable<UserPlayedEpisode>, bool>((e, _) => _addedEpisodes.AddRange(e))
                .Returns(Task.CompletedTask);
            _mocker.Setup<IUserPlayedEpisodeRepository, Task<UserPlayedEpisode>>(x => x.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync((UserPlayedEpisode)null);

            _addedMovies = new List<UserPlayedMovie>();
            _mocker.GetMock<IUserPlayedMovieRepository>()
                .Setup(x => x.AddRange(It.IsAny<IEnumerable<UserPlayedMovie>>(), It.IsAny<bool>()))
                .Callback<IEnumerable<UserPlayedMovie>, bool>((m, _) => _addedMovies.AddRange(m))
                .Returns(Task.CompletedTask);
            _mocker.Setup<IUserPlayedMovieRepository, Task<UserPlayedMovie>>(x => x.Get(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync((UserPlayedMovie)null);

            _mocker.Setup<IEmbyContentRepository, Task<EmbyContent>>(x => x.GetByEmbyId(It.IsAny<string>()))
                .ReturnsAsync(new EmbyContent { EmbyId = "series1", TheMovieDbId = "550", Type = MediaType.Series, Title = "Series" });

            SetupPlayedMovies();
            SetupPlayedEpisodes();

            _context = new Mock<IJobExecutionContext>();
            _context.Setup(x => x.MergedJobDataMap).Returns(new JobDataMap());

            _subject = _mocker.CreateInstance<EmbyPlayedSync>();
        }

        [Test]
        public async Task GarbageIndexNumberEnd_OnlyRecordsThePrimaryEpisode()
        {
            SetupPlayedEpisodes(Episode("ep1", episodeNumber: 1, indexNumberEnd: 406176));

            await _subject.Execute(_context.Object);

            Assert.That(_addedEpisodes, Has.Count.EqualTo(1));
            Assert.That(_addedEpisodes.Single().EpisodeNumber, Is.EqualTo(1));
        }

        [Test]
        public async Task IndexNumberEndBeforeIndexNumber_DoesNotFabricateEpisodes()
        {
            SetupPlayedEpisodes(Episode("ep1", episodeNumber: 5, indexNumberEnd: 3));

            await _subject.Execute(_context.Object);

            Assert.That(_addedEpisodes, Has.Count.EqualTo(1));
            Assert.That(_addedEpisodes.Single().EpisodeNumber, Is.EqualTo(5));
        }

        [Test]
        public async Task SaneMultiEpisodeRange_RecordsEveryEpisodeInTheFile()
        {
            SetupPlayedEpisodes(Episode("ep1", episodeNumber: 1, indexNumberEnd: 3));

            await _subject.Execute(_context.Object);

            Assert.That(_addedEpisodes.Select(x => x.EpisodeNumber), Is.EquivalentTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public async Task MovieWithNullProviderIds_IsSkippedWithoutCrashing()
        {
            SetupPlayedMovies(new EmbyMovie { Id = "movie1", Name = "No Metadata Movie", ProviderIds = null });

            await _subject.Execute(_context.Object);

            Assert.That(_addedMovies, Is.Empty);
            _mocker.Verify<INotificationHubService>(x => x.SendNotificationToAdmins("Emby Content Sync Failed", It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task EmptyPlayedPageWithRemainingRecords_StopsInsteadOfRefetchingForever()
        {
            _api.Setup(x => x.GetTvPlayed(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new EmbyItemContainer<EmbyEpisodes> { TotalRecordCount = 50, Items = new List<EmbyEpisodes>() });

            await _subject.Execute(_context.Object);

            _api.Verify(x => x.GetTvPlayed(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        private void SetupPlayedMovies(params EmbyMovie[] movies)
        {
            _api.Setup(x => x.GetMoviesPlayed(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new EmbyItemContainer<EmbyMovie>
                {
                    TotalRecordCount = movies.Length,
                    Items = movies.ToList()
                });
        }

        private void SetupPlayedEpisodes(params EmbyEpisodes[] episodes)
        {
            _api.Setup(x => x.GetTvPlayed(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new EmbyItemContainer<EmbyEpisodes>
                {
                    TotalRecordCount = episodes.Length,
                    Items = episodes.ToList()
                });
        }

        private static EmbyEpisodes Episode(string id, int episodeNumber, int? indexNumberEnd = null) => new EmbyEpisodes
        {
            Id = id,
            Name = $"Episode {id}",
            SeriesId = "series1",
            SeriesName = "Series",
            IndexNumber = episodeNumber,
            ParentIndexNumber = 1,
            IndexNumberEnd = indexNumberEnd,
            ProviderIds = null
        };
    }
}
