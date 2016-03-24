#region Copyright
/************************************************************************
   Copyright (c) 2016 Jamie Rees
   File: NotificationServiceTests.cs
   Created By: Jamie Rees

   Permission is hereby granted, free of charge, to any person obtaining
   a copy of this software and associated documentation files (the
   "Software"), to deal in the Software without restriction, including
   without limitation the rights to use, copy, modify, merge, publish,
   distribute, sublicense, and/or sell copies of the Software, and to
   permit persons to whom the Software is furnished to do so, subject to
   the following conditions:

   The above copyright notice and this permission notice shall be
   included in all copies or substantial portions of the Software.

   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
   EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
   MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
   NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
   LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
   OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
   WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 ************************************************************************/
#endregion
using System;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

using PlexRequests.Services.Interfaces;
using PlexRequests.Services.Notification;

namespace PlexRequests.Services.Tests
{
    [TestFixture]
    public class NotificationServiceTests
    {
        public NotificationService NotificationService { get; set; }

        [SetUp]
        public void Setup()
        {
            NotificationService = new NotificationService();
        }

        [Test]
        public void SubscribeNewNotifier()
        {
            var notificationMock = new Mock<INotification>();
            notificationMock.SetupGet(x => x.NotificationName).Returns("Notification1");
            NotificationService.Subscribe(notificationMock.Object);

            Assert.That(NotificationService.Observers["Notification1"], Is.Not.Null);
            Assert.That(NotificationService.Observers.Count, Is.EqualTo(1));
        }

        [Test]
        public void SubscribeExistingNotifier()
        {
            var notificationMock1 = new Mock<INotification>();
            var notificationMock2 = new Mock<INotification>();
            notificationMock1.SetupGet(x => x.NotificationName).Returns("Notification1");
            notificationMock2.SetupGet(x => x.NotificationName).Returns("Notification1");
            NotificationService.Subscribe(notificationMock1.Object);

            Assert.That(NotificationService.Observers["Notification1"], Is.Not.Null);
            Assert.That(NotificationService.Observers.Count, Is.EqualTo(1));

            NotificationService.Subscribe(notificationMock2.Object);

            Assert.That(NotificationService.Observers["Notification1"], Is.Not.Null);
            Assert.That(NotificationService.Observers.Count, Is.EqualTo(1));
        }

        [Test]
        public void UnSubscribeMissingNotifier()
        {
            var notificationMock = new Mock<INotification>();
            notificationMock.SetupGet(x => x.NotificationName).Returns("Notification1");
            NotificationService.UnSubscribe(notificationMock.Object);

            Assert.That(NotificationService.Observers.Count, Is.EqualTo(0));
        }

        [Test]
        public void UnSubscribeNotifier()
        {
            var notificationMock = new Mock<INotification>();
            notificationMock.SetupGet(x => x.NotificationName).Returns("Notification1");
            NotificationService.Subscribe(notificationMock.Object);
            Assert.That(NotificationService.Observers.Count, Is.EqualTo(1));

            NotificationService.UnSubscribe(notificationMock.Object);
            Assert.That(NotificationService.Observers.Count, Is.EqualTo(0));
        }

        [Test]
        public void PublishWithNoObservers()
        {
            Assert.DoesNotThrowAsync(
                async() =>
                { await NotificationService.Publish(new NotificationModel()); });
        }

        [Test]
        public async Task PublishAllNotifiers()
        {
            var notificationMock1 = new Mock<INotification>();
            var notificationMock2 = new Mock<INotification>();
            notificationMock1.SetupGet(x => x.NotificationName).Returns("Notification1");
            notificationMock2.SetupGet(x => x.NotificationName).Returns("Notification2");
            NotificationService.Subscribe(notificationMock1.Object);
            NotificationService.Subscribe(notificationMock2.Object);

            Assert.That(NotificationService.Observers.Count, Is.EqualTo(2));
            var model = new NotificationModel { Title = "abc", Body = "test" };
            await NotificationService.Publish(model);

            notificationMock1.Verify(x => x.NotifyAsync(model), Times.Once);
            notificationMock2.Verify(x => x.NotifyAsync(model), Times.Once);
        }

        [Test]
        public async Task PublishWithException()
        {
            var notificationMock = new Mock<INotification>();
            notificationMock.Setup(x => x.NotifyAsync(It.IsAny<NotificationModel>())).Throws<Exception>();
            notificationMock.SetupGet(x => x.NotificationName).Returns("Notification1");
            NotificationService.Subscribe(notificationMock.Object);
            await NotificationService.Publish(new NotificationModel());
        }
    }
}