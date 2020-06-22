using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Ombi.Core.Settings;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using System.Threading.Tasks;
using MockQueryable;
using MockQueryable.Moq;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class IssuesPurgeTests
    {

        [SetUp]
        public void Setup()
        {
            Repo = new Mock<IRepository<Issues>>();
            Settings = new Mock<ISettingsService<IssueSettings>>();
            Settings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new IssueSettings());
            Job = new IssuesPurge(Repo.Object, Settings.Object);
        }

        public Mock<IRepository<Issues>> Repo { get; set; }
        public Mock<ISettingsService<IssueSettings>> Settings { get; set; }
        public IssuesPurge Job { get; set; }

        [Test]
        public async Task DoesNotRun_WhenDisabled()
        {
            await Job.Execute(null);
            Repo.Verify(x => x.GetAll(),Times.Never);
        }

        [Test]
        public async Task Deletes_Correct_Issue()
        {
            var issues = new List<Issues>()
            {
                new Issues
                {
                    Status = IssueStatus.Resolved,
                    ResovledDate = DateTime.UtcNow.AddDays(-5).AddHours(-8)
                }
            };

            Settings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new IssueSettings { DeleteIssues = true, DaysAfterResolvedToDelete = 5 });
            Repo.Setup(x => x.GetAll()).Returns(new List<Issues>(issues).AsQueryable().BuildMock().Object);
            await Job.Execute(null);

            Assert.That(issues.First().Status, Is.EqualTo(IssueStatus.Deleted));
            Repo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task DoesNot_Delete_AllIssues()
        {
            var issues = new List<Issues>()
            {
                new Issues
                {
                    Status = IssueStatus.Resolved,
                    ResovledDate = DateTime.Now.AddDays(-2)
                },
                new Issues
                {
                    Status = IssueStatus.Resolved,
                    ResovledDate = DateTime.Now.AddDays(-6)
                }
            };

            Settings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new IssueSettings { DeleteIssues = true, DaysAfterResolvedToDelete = 5 });
            Repo.Setup(x => x.GetAll()).Returns(new EnumerableQuery<Issues>(issues).AsQueryable().BuildMock().Object);
            await Job.Execute(null);

            Assert.That(issues[0].Status, Is.Not.EqualTo(IssueStatus.Deleted));
            Assert.That(issues[1].Status, Is.EqualTo(IssueStatus.Deleted));
            Repo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task DoesNot_Delete_AnyIssues()
        {
            var issues = new List<Issues>()
            {
                new Issues
                {
                    Status = IssueStatus.Resolved,
                    ResovledDate = DateTime.Now.AddDays(-2)
                },
                new Issues
                {
                    Status = IssueStatus.Resolved,
                    ResovledDate = DateTime.Now.AddDays(-4)
                }
            };

            Settings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new IssueSettings { DeleteIssues = true, DaysAfterResolvedToDelete = 5 });
            Repo.Setup(x => x.GetAll()).Returns(new EnumerableQuery<Issues>(issues).AsQueryable().BuildMock().Object);
            await Job.Execute(null);

            Assert.That(issues[0].Status, Is.Not.EqualTo(IssueStatus.Deleted));
            Assert.That(issues[1].Status, Is.Not.EqualTo(IssueStatus.Deleted));
            Repo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}