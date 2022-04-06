using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Schedule.Jobs.Plex;
using Ombi.Store.Entities;
using Ombi.Test.Common;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class PlexWatchlistImportTests
    {

        private PlexWatchlistImport _subject;
        private AutoMocker _mocker;
        private Mock<IJobExecutionContext> _context;
        
        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker(); 
            var um = MockHelper.MockUserManager(new List<OmbiUser> { new OmbiUser { Id = "abc", UserType = UserType.PlexUser, MediaServerToken = "abc", UserName = "abc", NormalizedUserName = "ABC" } });
            _mocker.Use(um);
            _context = _mocker.GetMock<IJobExecutionContext>();
            _context.Setup(x => x.CancellationToken).Returns(CancellationToken.None);
            _subject = _mocker.CreateInstance<PlexWatchlistImport>();
        }

        [Test]
        public async Task TerminatesWhenPlexIsNotEnabled()
        {
            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings { Enable = false });
            await _subject.Execute(null);
        }
        
        [Test]
        public async Task EmptyWatchList()
        {
            _mocker.Setup<ISettingsService<PlexSettings>, Task<PlexSettings>>(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings { Enable = true });
            _mocker.Setup<IPlexApi, Task<PlexWatchlist>>(x => x.GetWatchlist(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PlexWatchlist());
            await _subject.Execute(_context.Object);
        }
    }
}
