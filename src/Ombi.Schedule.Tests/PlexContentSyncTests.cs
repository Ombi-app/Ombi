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
            var contentProcessed = new Dictionary<int, int>();
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
                        ratingKey = 1
                    },
                }
            };
            var contentToAdd = new HashSet<PlexServerContent>();
            var contentProcessed = new Dictionary<int, int>();

            await _subject.MovieLoop(new PlexServers(), content, contentToAdd, contentProcessed);

            var first = contentToAdd.First();
            Assert.That(first.ImdbId, Is.EqualTo("tt0322259"));
            _mocker.Verify<IPlexApi>(x => x.GetMetadata(It.IsAny<string>(), It.IsAny<string>(),It.IsAny<int>()), Times.Never);
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
                        ratingKey = 11,
                        title = "test1",
                        year = 2021,
                        type = "movie",
                    },
                }
            };
            var contentToAdd = new HashSet<PlexServerContent>();
            var contentProcessed = new Dictionary<int, int>();
            _mocker.Setup<IPlexApi>(x => x.GetMetadata(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new PlexMetadata
                {
                    MediaContainer = new Mediacontainer
                    {
                        Metadata = new[]
                        {
                            new Metadata
                            {
                                ratingKey = 11,
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

            await _subject.MovieLoop(new PlexServers { Ip = "http://test.com/", Port = 80}, content, contentToAdd, contentProcessed);

            var first = contentToAdd.First();
            Assert.That(first.ImdbId, Is.EqualTo("tt0322259"));

            _mocker.Verify<IPlexApi>(x => x.GetMetadata(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }
    }
}
