using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

using PlexRequestes.Automation.Helpers;

namespace PlexRequests.Automation.Pages
{
    public class UserLoginPage : BasePage
    {
        public UserLoginPage(IWebDriver webDriver)
        {
            WebDriver = webDriver;
            PageFactory.InitElements(WebDriver, this);
        }

        [FindsBy(How = How.Id, Using = "username")]
        public IWebElement Username { get; set; }
        [FindsBy(How = How.Id, Using = "password")]
        public IWebElement Password { get; set; }
        [FindsBy(How = How.Id, Using = "loginBtn")]
        public IWebElement Submit { get; set; }

        public SearchPage Login(string username, string password = "")
        {
            Username.SendKeys(username);

            if (Password.Exists(false) && !string.IsNullOrEmpty(password))
            {
                Password.SendKeys(password);
            }

            Submit.Click();
            return new SearchPage(WebDriver);
        }
    }
}
