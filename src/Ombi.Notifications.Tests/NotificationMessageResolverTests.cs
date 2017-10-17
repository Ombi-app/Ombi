using System.Linq;
using NUnit.Framework;
using Ombi.Store.Entities;

namespace Ombi.Notifications.Tests
{
    [TestFixture]
    public class NotificationMessageResolverTests
    {
        [SetUp]
        public void Setup()
        {

            _resolver = new NotificationMessageResolver();
        }
        
        private NotificationMessageResolver _resolver;

        [Test]
        public void Resolved_ShouldResolveSubject_RequestedUser()
        {
            var result = _resolver.ParseMessage(new NotificationTemplates
            {
                Subject = "This is a {RequestedUser}"
            }, new NotificationMessageCurlys { RequestedUser = "Abc" });
            Assert.True(result.Subject.Equals("This is a Abc"), result.Subject);
        }

        [Test]
        public void Resolved_ShouldResolveMessage_RequestedUser()
        {
            var result = _resolver.ParseMessage(new NotificationTemplates
            {
                Message = "This is a {RequestedUser}"
            }, new NotificationMessageCurlys { RequestedUser = "Abc" });
            Assert.True(result.Message.Equals("This is a Abc"), result.Message);
        }
    }
}
