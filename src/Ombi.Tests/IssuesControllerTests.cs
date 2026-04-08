using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MockQueryable.Moq;
using NUnit.Framework;
using Ombi.Controllers.V1;
using Ombi.Core;
using Ombi.Helpers;
using Ombi.Models;
using Ombi.Notifications.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Test.Common;

namespace Ombi.Tests
{
    [TestFixture]
    public class IssuesControllerTests
    {
        private Mock<IRepository<IssueCategory>> _categories;
        private Mock<IRepository<Issues>> _issues;
        private Mock<IRepository<IssueComments>> _comments;
        private Mock<INotificationHelper> _notification;
        private Mock<Ombi.Core.Authentication.OmbiUserManager> _userManager;
        private IssuesController _subject;

        [SetUp]
        public void Setup()
        {
            _categories = new Mock<IRepository<IssueCategory>>();
            _issues = new Mock<IRepository<Issues>>();
            _comments = new Mock<IRepository<IssueComments>>();
            _notification = new Mock<INotificationHelper>();

            var users = new List<OmbiUser>
            {
                new OmbiUser
                {
                    Id = "admin-id",
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    Alias = "admin"
                },
                new OmbiUser
                {
                    Id = "reporter-id",
                    UserName = "reporter",
                    NormalizedUserName = "REPORTER",
                    Alias = "reporter",
                    Email = "reporter@test.local"
                }
            };
            _userManager = MockHelper.MockUserManager(users);

            _subject = new IssuesController(_categories.Object, _issues.Object, _comments.Object, _userManager.Object, _notification.Object);
            _subject.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "admin") }))
                }
            };
        }

        [Test]
        public async Task UpdateStatus_InProgress_SendsIssueInProgressNotification()
        {
            var issue = new Issues
            {
                Id = 10,
                Title = "Issue title",
                Subject = "Issue subject",
                Description = "Issue description",
                RequestType = RequestType.Movie,
                Status = IssueStatus.Pending,
                UserReportedId = "reporter-id",
                UserReported = new OmbiUser
                {
                    Id = "reporter-id",
                    UserName = "reporter",
                    Alias = "reporter",
                    Email = "reporter@test.local"
                },
                IssueCategory = new IssueCategory { Id = 1, Value = "Playback" }
            };

            _issues.Setup(x => x.GetAll()).Returns(new List<Issues> { issue }.AsQueryable().BuildMock());
            _issues.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            _notification.Setup(x => x.Notify(It.IsAny<NotificationOptions>())).Returns(Task.CompletedTask);

            var result = await _subject.UpdateStatus(new IssueStateViewModel { IssueId = 10, Status = IssueStatus.InProgress });

            Assert.That(result, Is.True);
            _notification.Verify(x => x.Notify(It.Is<NotificationOptions>(n =>
                n.NotificationType == NotificationType.IssueInProgress &&
                n.UserId == "reporter-id" &&
                n.RequestType == RequestType.Movie &&
                n.Substitutes[NotificationSubstitues.Title] == "Issue title" &&
                n.Substitutes[NotificationSubstitues.IssueStatus] == IssueStatus.InProgress.ToString()
            )), Times.Once);
        }

        [Test]
        public async Task DeleteIssue_SendsIssueDeletedNotification()
        {
            var issue = new Issues
            {
                Id = 11,
                Title = "Issue to delete",
                Subject = "Delete subject",
                Description = "Delete description",
                RequestType = RequestType.TvShow,
                Status = IssueStatus.Pending,
                UserReportedId = "reporter-id",
                UserReported = new OmbiUser
                {
                    Id = "reporter-id",
                    UserName = "reporter",
                    Alias = "reporter",
                    Email = "reporter@test.local"
                },
                IssueCategory = new IssueCategory { Id = 2, Value = "Metadata" },
                Comments = new List<IssueComments>()
            };

            _issues.Setup(x => x.GetAll()).Returns(new List<Issues> { issue }.AsQueryable().BuildMock());
            _issues.Setup(x => x.Delete(issue)).Returns(Task.CompletedTask);
            _comments.Setup(x => x.GetAll()).Returns(new List<IssueComments>().AsQueryable().BuildMock());
            _comments.Setup(x => x.DeleteRange(It.IsAny<IEnumerable<IssueComments>>())).Returns(Task.CompletedTask);
            _notification.Setup(x => x.Notify(It.IsAny<NotificationOptions>())).Returns(Task.CompletedTask);

            var result = await _subject.DeleteIssue(11);

            Assert.That(result, Is.True);
            _notification.Verify(x => x.Notify(It.Is<NotificationOptions>(n =>
                n.NotificationType == NotificationType.IssueDeleted &&
                n.UserId == "reporter-id" &&
                n.RequestType == RequestType.TvShow &&
                n.Substitutes[NotificationSubstitues.Title] == "Issue to delete"
            )), Times.Once);
            _issues.Verify(x => x.Delete(issue), Times.Once);
        }

        [Test]
        public async Task DeleteIssue_DeletesAssociatedComments_BeforeDeletingIssue()
        {
            var comment1 = new IssueComments { Id = 1, IssuesId = 11, Comment = "First comment", UserId = "reporter-id" };
            var comment2 = new IssueComments { Id = 2, IssuesId = 11, Comment = "Second comment", UserId = "admin-id" };
            var unrelatedComment = new IssueComments { Id = 3, IssuesId = 99, Comment = "Other issue", UserId = "admin-id" };

            var issue = new Issues
            {
                Id = 11,
                Title = "Issue with comments",
                Subject = "Subject",
                Description = "Description",
                RequestType = RequestType.Movie,
                Status = IssueStatus.Pending,
                UserReportedId = "reporter-id",
                IssueCategory = new IssueCategory { Id = 1, Value = "Playback" },
                Comments = new List<IssueComments> { comment1, comment2 }
            };

            var callOrder = new List<string>();

            _issues.Setup(x => x.GetAll()).Returns(new List<Issues> { issue }.AsQueryable().BuildMock());
            _issues.Setup(x => x.Delete(issue)).Callback(() => callOrder.Add("DeleteIssue")).Returns(Task.CompletedTask);
            _comments.Setup(x => x.GetAll()).Returns(new List<IssueComments> { comment1, comment2, unrelatedComment }.AsQueryable().BuildMock());
            _comments.Setup(x => x.DeleteRange(It.IsAny<IEnumerable<IssueComments>>()))
                .Callback(() => callOrder.Add("DeleteComments"))
                .Returns(Task.CompletedTask);
            _notification.Setup(x => x.Notify(It.IsAny<NotificationOptions>())).Returns(Task.CompletedTask);

            var result = await _subject.DeleteIssue(11);

            Assert.That(result, Is.True);
            _comments.Verify(x => x.DeleteRange(It.Is<IEnumerable<IssueComments>>(c =>
                c.Count() == 2 && c.Contains(comment1) && c.Contains(comment2) && !c.Contains(unrelatedComment)
            )), Times.Once);
            _issues.Verify(x => x.Delete(issue), Times.Once);
            Assert.That(callOrder, Is.EqualTo(new List<string> { "DeleteComments", "DeleteIssue" }));
        }

        [Test]
        public async Task DeleteIssue_ReturnsFalseAndDoesNotNotify_WhenIssueNotFound()
        {
            _issues.Setup(x => x.GetAll()).Returns(new List<Issues>().AsQueryable().BuildMock());

            var result = await _subject.DeleteIssue(999);

            Assert.That(result, Is.False);
            _notification.Verify(x => x.Notify(It.IsAny<NotificationOptions>()), Times.Never);
            _issues.Verify(x => x.Delete(It.IsAny<Issues>()), Times.Never);
        }

        [Test]
        public async Task UpdateStatus_DoesNotNotify_WhenStatusIsPending()
        {
            var issue = new Issues
            {
                Id = 12,
                Title = "Issue title",
                Subject = "Issue subject",
                Description = "Issue description",
                RequestType = RequestType.Movie,
                Status = IssueStatus.Pending,
                UserReportedId = "reporter-id",
                UserReported = new OmbiUser
                {
                    Id = "reporter-id",
                    UserName = "reporter",
                    Alias = "reporter",
                    Email = "reporter@test.local"
                },
                IssueCategory = new IssueCategory { Id = 1, Value = "Playback" }
            };

            _issues.Setup(x => x.GetAll()).Returns(new List<Issues> { issue }.AsQueryable().BuildMock());
            _issues.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _subject.UpdateStatus(new IssueStateViewModel { IssueId = 12, Status = IssueStatus.Pending });

            Assert.That(result, Is.True);
            _notification.Verify(x => x.Notify(It.IsAny<NotificationOptions>()), Times.Never);
        }

        [Test]
        public async Task UpdateStatus_InProgress_UsesFallbackAlias_WhenUserAliasNull()
        {
            var issue = new Issues
            {
                Id = 13,
                Title = "Issue title",
                Subject = "Issue subject",
                Description = "Issue description",
                RequestType = RequestType.Movie,
                Status = IssueStatus.Pending,
                UserReportedId = "reporter-id",
                UserReported = new OmbiUser
                {
                    Id = "reporter-id",
                    UserName = "reporter",
                    Alias = null,
                    Email = "reporter@test.local"
                },
                IssueCategory = new IssueCategory { Id = 1, Value = "Playback" }
            };

            _issues.Setup(x => x.GetAll()).Returns(new List<Issues> { issue }.AsQueryable().BuildMock());
            _issues.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            _notification.Setup(x => x.Notify(It.IsAny<NotificationOptions>())).Returns(Task.CompletedTask);

            var result = await _subject.UpdateStatus(new IssueStateViewModel { IssueId = 13, Status = IssueStatus.InProgress });

            Assert.That(result, Is.True);
            _notification.Verify(x => x.Notify(It.Is<NotificationOptions>(n =>
                n.NotificationType == NotificationType.IssueInProgress &&
                n.Substitutes[NotificationSubstitues.IssueUserAlias] == "reporter"
            )), Times.Once);
        }
    }
}
