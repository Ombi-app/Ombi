using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq.AutoMock;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ombi.Notifications.Tests
{
    [TestFixture]
    public class NotificationServiceTests
    {

        private NotitficationServiceTestFacade _subject;

        [SetUp]
        public void Setup()
        {
            var mocker = new AutoMocker();
            mocker.Use(NullLogger.Instance);
            _subject = mocker.CreateInstance<NotitficationServiceTestFacade>();
        }

        [Test]
        public void PopulateAgentsTests()
        {
            Assert.That(_subject.Agents, Has.Count.EqualTo(13));
            Assert.That(_subject.Agents.DistinctBy(x => x.NotificationName).ToList(), Has.Count.EqualTo(13));
        }
    }


    public class NotitficationServiceTestFacade : NotificationService
    {
        public NotitficationServiceTestFacade(IServiceProvider provider, ILogger<NotificationService> log) : base(provider, log)
        {
        }

        public List<INotification> Agents => base.NotificationAgents;
    }
}
