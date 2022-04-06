using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Settings.Models.External;
using Ombi.Schedule.Jobs.Plex;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class PlexContentSyncTests
    {

        private AutoMocker _mocker;
        private PlexContentSync _subject;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            _subject = _mocker.CreateInstance<PlexContentSync>();
        }

        [Test]
        public async Task DoesNotSyncExistingMovie()
        {
            var content = new Mediacontainer
            {
                Metadata = new[]
                {
                    new Metadata
                    {
                        title = "test1",
                        year = 2021,
                        type = "movie"
                    },
                }
            };
            var contentToAdd = new HashSet<PlexServerContent>();
            var contentProcessed = new Dictionary<int, string>();
            _mocker.Setup<IPlexContentRepository>(x =>
                    x.GetFirstContentByCustom(It.IsAny<Expression<Func<PlexServerContent, bool>>>()))
                .Returns(Task.FromResult(new PlexServerContent()));

            await _subject.MovieLoop(new PlexServers(), content, contentToAdd, contentProcessed);

            Assert.That(contentToAdd, Is.Empty);
        }

        [Test]
        public async Task SyncsMovieWithGuidFromInitalMetadata()
        {
            var content = new Mediacontainer
            {
                Metadata = new[]
                {
                    new Metadata
                    {
                        title = "test1",
                        year = 2021,
                        type = "movie",
                        Guid = new List<PlexGuids>
                        {
                            new PlexGuids
                            {
                                Id = "imdb://tt0322259"
                            }
                        },
                        ratingKey = "1"
                    },
                }
            };
            var contentToAdd = new HashSet<PlexServerContent>();
            var contentProcessed = new Dictionary<int, string>();

            await _subject.MovieLoop(new PlexServers(), content, contentToAdd, contentProcessed);

            var first = contentToAdd.First();
            Assert.That(first.ImdbId, Is.EqualTo("tt0322259"));
            _mocker.Verify<IPlexApi>(x => x.GetMetadata(It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SyncsMovieWithGuidFromCallToApi()
        {
            var content = new Mediacontainer
            {
                Metadata = new[]
                {
                    new Metadata
                    {
                        ratingKey = "11",
                        title = "test1",
                        year = 2021,
                        type = "movie",
                    },
                }
            };
            var contentToAdd = new HashSet<PlexServerContent>();
            var contentProcessed = new Dictionary<int, string>();
            _mocker.Setup<IPlexApi>(x => x.GetMetadata(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new PlexMetadata
                {
                    MediaContainer = new Mediacontainer
                    {
                        Metadata = new[]
                        {
                            new Metadata
                            {
                                ratingKey = "11",
                                title = "test1",
                                year = 2021,
                                type = "movie",
                                Guid = new List<PlexGuids>
                                {
                                    new PlexGuids
                                    {
                                        Id = "imdb://tt0322259"
                                    }
                                },
                            }
                        }
                    }
                }));

            await _subject.MovieLoop(new PlexServers { Ip = "http://test.com/", Port = 80 }, content, contentToAdd, contentProcessed);

            var first = contentToAdd.First();
            Assert.That(first.ImdbId, Is.EqualTo("tt0322259"));

            _mocker.Verify<IPlexApi>(x => x.GetMetadata(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task UpdatesExistingMovieWhen_WeFindAnotherQuality()
        {
            var content = new Mediacontainer
            {
                Metadata = new[]
                {
                    new Metadata
                    {
                        ratingKey = "11",
                        title = "test1",
                        year = 2021,
                        type = "movie",
                        Media = new Medium[1]
                        {
                            new Medium
                            {
                                videoResolution = "4k"
                            }
                        }
                    },
                }
            };
            var contentToAdd = new HashSet<PlexServerContent>();
            var contentProcessed = new Dictionary<int, string>();
            _mocker.Setup<IPlexContentRepository>(x =>
                    x.GetFirstContentByCustom(It.IsAny<Expression<Func<PlexServerContent, bool>>>()))
                .Returns(Task.FromResult(new PlexServerContent
                {
                    Quality = "1080"
                }));

            await _subject.MovieLoop(new PlexServers(), content, contentToAdd, contentProcessed);

            Assert.That(contentToAdd, Is.Empty);
            _mocker.Verify<IPlexContentRepository>(x => x.Update(It.Is<PlexServerContent>(x => x.Quality == "1080" && x.Has4K)), Times.Once);
        }

        [Test]
        public async Task DoesNotUpdatesExistingMovieWhen_WeFindSameQuality()
        {
            var content = new Mediacontainer
            {
                Metadata = new[]
                {
                    new Metadata
                    {
                        ratingKey = "11",
                        title = "test1",
                        year = 2021,
                        type = "movie",
                        Media = new Medium[1]
                        {
                            new Medium
                            {
                                videoResolution = "1080"
                            }
                        }
                    },
                }
            };
            var contentToAdd = new HashSet<PlexServerContent>();
            var contentProcessed = new Dictionary<int, string>();
            _mocker.Setup<IPlexContentRepository>(x =>
                    x.GetFirstContentByCustom(It.IsAny<Expression<Func<PlexServerContent, bool>>>()))
                .Returns(Task.FromResult(new PlexServerContent
                {
                    Quality = "1080"
                }));

            await _subject.MovieLoop(new PlexServers(), content, contentToAdd, contentProcessed);

            Assert.That(contentToAdd, Is.Empty);
            _mocker.Verify<IPlexContentRepository>(x => x.Update(It.IsAny<PlexServerContent>()), Times.Never);
        }
    }
}
