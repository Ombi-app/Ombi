#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: SearchPage.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

using PlexRequestes.Automation.Helpers;

namespace PlexRequests.Automation.Pages
{
    public class SearchPage : BasePage
    {
        public SearchPage(IWebDriver webDriver)
        {
            WebDriver = webDriver;
            PageFactory.InitElements(WebDriver, this);

            while (!PageTitle.Exists())
            {
                Thread.Sleep(500);
                PageFactory.InitElements(WebDriver, this);
            }
        }

        [FindsBy(How = How.Id, Using = "searchTitle")]
        public IWebElement PageTitle { get; set; }

        [FindsBy(How = How.Id, Using = "movieSearchContent")]
        public IWebElement SearchBox { get; set; }

        [FindsBy(How = How.Id, Using = "movieTabButton")]
        public IWebElement MovieTab { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id=\"movieList\"]/div")]
        public IList<IWebElement> MovieResults { get; set; }

        public SearchPage SearchForMovie(string movie)
        {
            MovieTab.Click();

            SearchBox.SendKeys(movie);

            while (MovieResults.Count < 0)
            {
                Thread.Sleep(500);
                PageFactory.InitElements(WebDriver, this);
            }

            return this;
        }

        public bool RequestMovie(IWebElement movieElement)
        {
            var request = movieElement.FindElement(By.XPath(".//div[3]/form/button"));
            request.Click();

            PageFactory.InitElements(WebDriver, this);

            return Notificaiton.Exists();
        }
    }
}