#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: UserRequestLimitResetterTests.cs
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

using Moq;

using NUnit.Framework;

using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Services.Interfaces;
using PlexRequests.Services.Jobs;
using PlexRequests.Store;
using PlexRequests.Store.Models;
using PlexRequests.Store.Repository;

using Ploeh.AutoFixture;

using Quartz;

namespace PlexRequests.Services.Tests
{
    [TestFixture]
    public class UserRequestLimitResetterTests
    {
        [SetUp]
        public void Setup()
        {
            F = new Fixture();
            JobMock = new Mock<IJobRecord>();
            RepoMock = new Mock<IRepository<RequestLimit>>();
            SettingsMock = new Mock<ISettingsService<PlexRequestSettings>>();
            ContextMock = new Mock<IJobExecutionContext>();

            SettingsMock.Setup(x => x.GetSettings()).Returns(new PlexRequestSettings());

            Resetter = new UserRequestLimitResetter(JobMock.Object, RepoMock.Object, SettingsMock.Object);
        }

        [TearDown]
        public void Teardown()
        {
            SettingsMock.Verify(x => x.GetSettings(), Times.Once);
            JobMock.Verify(x => x.Record(It.IsAny<string>()), Times.Once());
        }

        public UserRequestLimitResetter Resetter { get; set; }
        private Mock<IJobRecord> JobMock { get; set; }
        private Mock<IRepository<RequestLimit>> RepoMock { get; set; }
        private Mock<ISettingsService<PlexRequestSettings>> SettingsMock { get; set; }
        private Mock<IJobExecutionContext> ContextMock { get; set; }
        private Fixture F { get; set; }

        [TestCaseSource(nameof(ResetData))]
        public void Reset(int movie, int tv, int album, RequestType type)
        {
            SetupSettings(movie, tv, album);
            RepoMock.Setup(x => x.GetAll())
                    .Returns(F.Build<RequestLimit>().With(x => x.FirstRequestDate, DateTime.Now.AddDays(-8)).With(x => x.RequestType, type).CreateMany());

            Resetter = new UserRequestLimitResetter(JobMock.Object, RepoMock.Object, SettingsMock.Object);

            Resetter.Execute(ContextMock.Object);

            RepoMock.Verify(x => x.Delete(It.IsAny<RequestLimit>()), Times.Exactly(3));
        }

        [TestCaseSource(nameof(DoNotResetData))]
        public void DoNotReset(int days, RequestType type)
        {
            SetupSettings(1, 1, 1);
            RepoMock.Setup(x => x.GetAll())
                    .Returns(F.Build<RequestLimit>().With(x => x.FirstRequestDate, DateTime.Now.AddDays(days)).With(x => x.RequestType, type).CreateMany());

            Resetter = new UserRequestLimitResetter(JobMock.Object, RepoMock.Object, SettingsMock.Object);

            Resetter.Execute(ContextMock.Object);

            RepoMock.Verify(x => x.Delete(It.IsAny<RequestLimit>()), Times.Never);
        }

        static readonly IEnumerable<TestCaseData> ResetData = new List<TestCaseData>
        {
            new TestCaseData(1, 0, 0, RequestType.Movie).SetName("Reset Movies"),
            new TestCaseData(0, 1, 0, RequestType.TvShow).SetName("Reset TV Shows"),
            new TestCaseData(0, 0, 1, RequestType.Album).SetName("Reset Albums"),
            new TestCaseData(1, 1, 1, RequestType.Album).SetName("Reset Albums with all enabled")
        };

        private static readonly IEnumerable<TestCaseData> DoNotResetData = new List<TestCaseData>
        {
            new TestCaseData(1, RequestType.Movie).SetName("1 Day(s)"),
            new TestCaseData(-6, RequestType.TvShow).SetName("-6 Day(s)"),
            new TestCaseData(-1, RequestType.TvShow).SetName("-1 Day(s)"),
            new TestCaseData(-2, RequestType.Album).SetName("-2 Day(s)"),
            new TestCaseData(-3, RequestType.TvShow).SetName("-3 Day(s)"),
            new TestCaseData(-4, RequestType.Movie).SetName("-4 Day(s)"),
            new TestCaseData(-5, RequestType.TvShow).SetName("-5 Day(s)"),
            new TestCaseData(-6, RequestType.Movie).SetName("-6 Day(s)"),
            new TestCaseData(0, RequestType.TvShow).SetName("0 Day(s)")
        };

        private void SetupSettings(int movie, int tv, int album)
        {
            SettingsMock.Setup(x => x.GetSettings())
                        .Returns(new PlexRequestSettings { MovieWeeklyRequestLimit = movie, TvWeeklyRequestLimit = tv, AlbumWeeklyRequestLimit = album });
        }

        [Test]
        public void ResetTurnedOff()
        {
            SetupSettings(0, 0, 0);
            Resetter = new UserRequestLimitResetter(JobMock.Object, RepoMock.Object, SettingsMock.Object);

            Resetter.Execute(ContextMock.Object);

            RepoMock.Verify(x => x.Delete(It.IsAny<RequestLimit>()), Times.Never);
        }
    }
}