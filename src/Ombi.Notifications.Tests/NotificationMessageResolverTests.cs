//using System.Linq;
//using Ombi.Store.Entities;
//using Xunit;
//using Xunit.Abstractions;

//namespace Ombi.Notifications.Tests
//{
//    public class NotificationMessageResolverTests
//    {
//        public NotificationMessageResolverTests(ITestOutputHelper helper)
//        {
//            _resolver = new NotificationMessageResolver();
//            output = helper;
//        }

//        private readonly NotificationMessageResolver _resolver;
//        private readonly ITestOutputHelper output;

//        [Fact]
//        public void Resolved_ShouldResolve_RequestedUser()
//        {
//            var result = _resolver.ParseMessage(new NotificationTemplates
//            {
//                Subject = "This is a {RequestedUser}"
//            }, new NotificationMessageCurlys {RequestedUser = "Abc"});
//            output.WriteLine(result.Message);
//            //Assert.True(result.Message.Equals("This is a Abc"));
            
//            Assert.Contains("11a", result.Message);
//        }
//    }
//}
