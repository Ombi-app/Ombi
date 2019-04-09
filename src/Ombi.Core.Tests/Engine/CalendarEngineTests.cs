
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Moq;
using NUnit.Framework;
using Ombi.Core.Authentication;
using Ombi.Core.Engine.V2;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Tests.Engine
{
    [TestFixture]
    public class CalendarEngineTests
    {
        public Mock<IMovieRequestRepository> MovieRepo { get; set; }
        public Mock<ITvRequestRepository> TvRepo { get; set; }
        public CalendarEngine CalendarEngine { get; set; }

        [SetUp]
        public void Setup()
        {
            MovieRepo = new Mock<IMovieRequestRepository>();
            TvRepo = new Mock<ITvRequestRepository>();
            var principle = new Mock<IPrincipal>();
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.Name).Returns("UnitTest");
            principle.Setup(x => x.Identity).Returns(identity.Object);
            CalendarEngine = new CalendarEngine(principle.Object, null, null, MovieRepo.Object, TvRepo.Object);
        }

        [Test]
        public async Task Calendar_Movies_OnlyGet_PreviousAndFuture_90_Days()
        {
            var movies = new List<MovieRequests>
            {
                new MovieRequests
                {
                    Title="Invalid",
                    ReleaseDate = new DateTime(2018,10,01)
                },
                new MovieRequests
                {
                    Title="Invalid",
                    ReleaseDate = DateTime.Now.AddDays(91)
                },

                new MovieRequests
                {
                    Title="Valid",
                    ReleaseDate = DateTime.Now
                }
            };
            MovieRepo.Setup(x => x.GetAll()).Returns(movies.AsQueryable());
            var data = await CalendarEngine.GetCalendarData();

            Assert.That(data.Count, Is.EqualTo(1));
            Assert.That(data[0].Title, Is.EqualTo("Valid"));
        }

        [Test]
        public async Task Calendar_Episodes_OnlyGet_PreviousAndFuture_90_Days()
        {
            var tv = new List<ChildRequests>
            {
                new ChildRequests
                {
                    SeasonRequests = new List<SeasonRequests>
                    {
                        new SeasonRequests
                        {
                            Episodes = new List<EpisodeRequests>
                            {
                                new EpisodeRequests
                                {
                                    Title = "Invalid",
                                    AirDate = new DateTime(2018,01,01)
                                },
                                new EpisodeRequests
                                {
                                    Title = "Invalid",
                                    AirDate = DateTime.Now.AddDays(91)
                                },
                                new EpisodeRequests
                                {
                                    Title = "Valid",
                                    AirDate = DateTime.Now
                                },
                            }
                        }
                    }
                },
            };
            TvRepo.Setup(x => x.GetChild()).Returns(tv.AsQueryable());
            var data = await CalendarEngine.GetCalendarData();

            Assert.That(data.Count, Is.EqualTo(1));
            Assert.That(data[0].Title, Is.EqualTo("Valid"));
        }


        [TestCaseSource(nameof(StatusTvColorData))]
        public async Task<string> Calendar_Tv_StatusColor(AvailabilityTestModel model)
        {
            var tv = new List<ChildRequests>
            {
                new ChildRequests
                {
                    SeasonRequests = new List<SeasonRequests>
                    {
                        new SeasonRequests
                        {
                            Episodes = new List<EpisodeRequests>
                            {
                                new EpisodeRequests
                                {
                                    Title = "Valid",
                                    AirDate = DateTime.Now,
                                    Approved = model.Approved,
                                    Available = model.Available
                                },
                            }
                        }
                    }
                },
            };
            TvRepo.Setup(x => x.GetChild()).Returns(tv.AsQueryable());
            var data = await CalendarEngine.GetCalendarData();

            return data[0].BackgroundColor;
        }

        [TestCaseSource(nameof(StatusColorData))]
        public async Task<string> Calendar_Movie_StatusColor(AvailabilityTestModel model)
        {
            var movies = new List<MovieRequests>
            {
                new MovieRequests
                {
                    Title="Valid",
                    ReleaseDate = DateTime.Now,
                    Denied = model.Denied,
                    Approved = model.Approved,
                    Available = model.Available
                },
            };
            MovieRepo.Setup(x => x.GetAll()).Returns(movies.AsQueryable());
            var data = await CalendarEngine.GetCalendarData();

            return data[0].BackgroundColor;
        }

        public static IEnumerable<TestCaseData> StatusColorData
        {
            get
            {
                yield return new TestCaseData(new AvailabilityTestModel
                {
                    Approved = true,
                    Denied = true
                }).Returns("red").SetName("Calendar_DeniedRequest");
                foreach (var testCaseData in StatusTvColorData)
                {
                    yield return testCaseData;
                }
            }
        }

        public static IEnumerable<TestCaseData> StatusTvColorData
        {
            get
            {
                yield return new TestCaseData(new AvailabilityTestModel
                {
                    Available = true,
                    Approved = true
                }).Returns("#469c83").SetName("Calendar_AvailableRequest");
                yield return new TestCaseData(new AvailabilityTestModel
                {
                    Approved = true
                }).Returns("blue").SetName("Calendar_ApprovedRequest");
            }
        }
    }

    public class AvailabilityTestModel
    {
        public bool Available { get; set; }
        public bool Denied { get; set; }
        public bool Approved { get; set; }
    }
}
