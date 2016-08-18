using NUnit.Framework;

using PlexRequestes.Automation.Helpers;

using PlexRequests.Automation.Pages;

namespace PlexRequests.Automation
{
    [TestFixture]
    public class UserLoginTests : AutomationTestBase
    {

        public UserLoginTests() : base("http://localhost:8080")
        {
        }

        [Test]
        [Ignore("Cannot work with CI Build currently")]
        public void LoginWithoutAuthentication()
        {
            using (Driver)
            {
                var userLogin = new UserLoginPage(Driver);
                var search = userLogin.Login("AutomationUser");

                Assert.That(search.PageTitle.Exists());
            }
        }

        [Test]
        [Ignore("Cannot work with CI Build currently")]
        public void SearchAndRequestMovie()
        {
            using (Driver)
            {
                var userLogin = new UserLoginPage(Driver);
                var search = userLogin.Login("AutomationUser");

                search.SearchForMovie("007");
            }
        }
    }
}
    