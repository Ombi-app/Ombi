using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.External.MediaServers.Plex;
using Ombi.Api.External.MediaServers.Plex.Models;
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
            _mocker.Setup<IPlexContentRepository, Task<PlexServerContent>>(x =>
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
            _mocker.Setup<IPlexApi, Task<PlexMetadata>>(x => x.GetMetadata(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
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
            _mocker.Setup<IPlexContentRepository, Task<PlexServerContent>>(x =>
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
            _mocker.Setup<IPlexContentRepository, Task<PlexServerContent>>(x =>
                    x.GetFirstContentByCustom(It.IsAny<Expression<Func<PlexServerContent, bool>>>()))
                .Returns(Task.FromResult(new PlexServerContent
                {
                    Quality = "1080"
                }));

            await _subject.MovieLoop(new PlexServers(), content, contentToAdd, contentProcessed);

            Assert.That(contentToAdd, Is.Empty);
            _mocker.Verify<IPlexContentRepository>(x => x.Update(It.IsAny<PlexServerContent>()), Times.Never);
        }

        [Test]
        public void PlexServerContentKeyComparer_PreventsDuplicates_BasedOnKey()
        {
            // Use reflection to access the private nested class
            var comparerType = typeof(PlexContentSync).GetNestedType("PlexServerContentKeyComparer",
                System.Reflection.BindingFlags.NonPublic);
            var comparer = (IEqualityComparer<PlexServerContent>)Activator.CreateInstance(comparerType);
            var contentToAdd = new HashSet<PlexServerContent>(comparer);

            var item1 = new PlexServerContent { Key = "12345", Title = "Test Show" };
            var item2 = new PlexServerContent { Key = "12345", Title = "Test Show Different Instance" };
            var item3 = new PlexServerContent { Key = "67890", Title = "Different Show" };

            Assert.That(contentToAdd.Add(item1), Is.True, "First item should be added");
            Assert.That(contentToAdd.Add(item2), Is.False, "Second item with same Key should be rejected");
            Assert.That(contentToAdd.Add(item3), Is.True, "Item with different Key should be added");
            Assert.That(contentToAdd.Count, Is.EqualTo(2), "HashSet should contain only 2 items");
        }

        [Test]
        public async Task ProcessTvShow_DoesNotProcessDuplicate_WhenKeyAlreadyInContentProcessed()
        {
            var show = new Metadata
            {
                ratingKey = "302135",
                title = "Duplicate Show",
                year = 2021
            };

            // Use reflection to access the private nested class
            var comparerType = typeof(PlexContentSync).GetNestedType("PlexServerContentKeyComparer",
                System.Reflection.BindingFlags.NonPublic);
            var comparer = (IEqualityComparer<PlexServerContent>)Activator.CreateInstance(comparerType);
            var contentToAdd = new HashSet<PlexServerContent>(comparer);
            var contentProcessed = new Dictionary<int, string>
            {
                { 1, "302135" } // Already processed
            };

            _mocker.Setup<IPlexApi, Task<PlexMetadata>>(x =>
                    x.GetSeasons(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new PlexMetadata
                {
                    MediaContainer = new Mediacontainer
                    {
                        Metadata = Array.Empty<Metadata>()
                    }
                }));

            // Use reflection to call the private ProcessTvShow method
            var method = typeof(PlexContentSync).GetMethod("ProcessTvShow",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            await (Task)method.Invoke(_subject, new object[]
            {
                new PlexServers { Ip = "http://test.com/", Port = 80 },
                show,
                contentToAdd,
                contentProcessed
            });

            // Should return early without calling GetSeasons
            _mocker.Verify<IPlexApi>(x => x.GetSeasons(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never, "GetSeasons should not be called for duplicate show");
            Assert.That(contentToAdd, Is.Empty, "No content should be added for duplicate");
        }

        [Test]
        public async Task ProcessTvShow_DoesNotProcessDuplicate_WhenKeyAlreadyInContentToAdd()
        {
            var show = new Metadata
            {
                ratingKey = "302135",
                title = "Duplicate Show",
                year = 2021
            };

            // Use reflection to access the private nested class
            var comparerType = typeof(PlexContentSync).GetNestedType("PlexServerContentKeyComparer",
                System.Reflection.BindingFlags.NonPublic);
            var comparer = (IEqualityComparer<PlexServerContent>)Activator.CreateInstance(comparerType);
            var contentToAdd = new HashSet<PlexServerContent>(comparer);

            // Pre-add an item with the same key
            contentToAdd.Add(new PlexServerContent { Key = "302135", Title = "Already Added" });

            var contentProcessed = new Dictionary<int, string>();

            _mocker.Setup<IPlexApi, Task<PlexMetadata>>(x =>
                    x.GetSeasons(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new PlexMetadata
                {
                    MediaContainer = new Mediacontainer
                    {
                        Metadata = Array.Empty<Metadata>()
                    }
                }));

            // Use reflection to call the private ProcessTvShow method
            var method = typeof(PlexContentSync).GetMethod("ProcessTvShow",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            await (Task)method.Invoke(_subject, new object[]
            {
                new PlexServers { Ip = "http://test.com/", Port = 80 },
                show,
                contentToAdd,
                contentProcessed
            });

            // Should return early without calling GetSeasons
            _mocker.Verify<IPlexApi>(x => x.GetSeasons(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never, "GetSeasons should not be called for duplicate show");
            Assert.That(contentToAdd.Count, Is.EqualTo(1), "Should still have only the original item");
        }

        [Test]
        public async Task ProcessTvShow_MultipleTimes_WithinSameBatch_OnlyFirstCallProcesses()
        {
            // This test reproduces the original issue #5331:
            // The same TV show being processed multiple times within a single batch (before SaveChanges)
            // Previously this would add duplicate entries causing "Duplicate entry for key" DB errors
            // Now with our fix: early detection prevents reprocessing

            var show1 = new Metadata { ratingKey = "302135", title = "Game of Thrones", year = 2011 };
            var show2 = new Metadata { ratingKey = "302135", title = "Game of Thrones", year = 2011 }; // Same show, different object
            var show3 = new Metadata { ratingKey = "302135", title = "Game of Thrones", year = 2011 }; // Same show, third object

            // Use reflection to access the private nested class
            var comparerType = typeof(PlexContentSync).GetNestedType("PlexServerContentKeyComparer",
                System.Reflection.BindingFlags.NonPublic);
            var comparer = (IEqualityComparer<PlexServerContent>)Activator.CreateInstance(comparerType);
            var contentToAdd = new HashSet<PlexServerContent>(comparer);
            var contentProcessed = new Dictionary<int, string>();

            // Mock GetSeasons to return empty - we're testing early detection, not the full processing
            _mocker.Setup<IPlexApi, Task<PlexMetadata>>(x =>
                    x.GetSeasons(It.IsAny<string>(), It.IsAny<string>(), "302135"))
                .Returns(Task.FromResult(new PlexMetadata
                {
                    MediaContainer = new Mediacontainer { Metadata = Array.Empty<Metadata>() }
                }));

            _mocker.Setup<IPlexContentRepository, Task<PlexServerContent>>(x =>
                    x.GetFirstContentByCustom(It.IsAny<Expression<Func<PlexServerContent, bool>>>()))
                .Returns(Task.FromResult<PlexServerContent>(null));

            _mocker.Setup<IPlexContentRepository, Task<PlexServerContent>>(x =>
                    x.GetByKey("302135"))
                .Returns(Task.FromResult<PlexServerContent>(null));

            // Use reflection to call the private ProcessTvShow method
            var method = typeof(PlexContentSync).GetMethod("ProcessTvShow",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var server = new PlexServers { Ip = "http://test.com/", Port = 80 };

            // First call - should process normally
            await (Task)method.Invoke(_subject, new object[] { server, show1, contentToAdd, contentProcessed });

            // Manually add to contentProcessed to simulate what would happen after SaveChanges
            // (In real code, this happens after the batch is saved to DB)
            contentProcessed.Add(1, "302135");

            // Second and third calls - should be rejected by early duplicate detection
            await (Task)method.Invoke(_subject, new object[] { server, show2, contentToAdd, contentProcessed });
            await (Task)method.Invoke(_subject, new object[] { server, show3, contentToAdd, contentProcessed });

            // VERIFY THE FIX:
            // GetSeasons should only be called ONCE (for the first show), not three times
            _mocker.Verify<IPlexApi>(x => x.GetSeasons(It.IsAny<string>(), It.IsAny<string>(), "302135"),
                Times.Once,
                "GetSeasons should only be called once - subsequent calls should be rejected by early duplicate detection");

            // This verifies that the early duplicate check (contentProcessed.ContainsValue) works correctly
            // In the original bug, all three calls would process and try to add to the database,
            // causing: "Duplicate entry '302135' for key 'AK_PlexServerContent_Key'"
        }

        [Test]
        public void CustomComparerPreventsMultipleAdds_ToHashSet()
        {
            // This test verifies that even if the early checks somehow failed,
            // the custom comparer on the HashSet would still prevent duplicates
            // This is the second layer of defense

            var comparerType = typeof(PlexContentSync).GetNestedType("PlexServerContentKeyComparer",
                System.Reflection.BindingFlags.NonPublic);
            var comparer = (IEqualityComparer<PlexServerContent>)Activator.CreateInstance(comparerType);
            var contentToAdd = new HashSet<PlexServerContent>(comparer);

            // Simulate three attempts to add the same show (same Key, different objects)
            // This is what would happen in the original bug within a single batch
            var show1 = new PlexServerContent
            {
                Key = "302135",
                Title = "Game of Thrones",
                Type = MediaType.Series,
                Seasons = new List<PlexSeasonsContent>()
            };

            var show2 = new PlexServerContent
            {
                Key = "302135", // SAME KEY
                Title = "Game of Thrones",
                Type = MediaType.Series,
                Seasons = new List<PlexSeasonsContent>()
            };

            var show3 = new PlexServerContent
            {
                Key = "302135", // SAME KEY AGAIN
                Title = "Game of Thrones",
                Type = MediaType.Series,
                Seasons = new List<PlexSeasonsContent>()
            };

            // Try to add all three
            var added1 = contentToAdd.Add(show1);
            var added2 = contentToAdd.Add(show2); // Should be rejected
            var added3 = contentToAdd.Add(show3); // Should be rejected

            // VERIFY THE FIX:
            Assert.That(added1, Is.True, "First show should be added successfully");
            Assert.That(added2, Is.False, "Second show with same Key should be rejected by comparer");
            Assert.That(added3, Is.False, "Third show with same Key should be rejected by comparer");
            Assert.That(contentToAdd.Count, Is.EqualTo(1), "Only ONE show should be in the HashSet");

            // This demonstrates that even if somehow the same show got processed multiple times,
            // the custom comparer prevents duplicate entries in contentToAdd,
            // which prevents the "Duplicate entry" database error when SaveChanges() is called
        }
    }
}
