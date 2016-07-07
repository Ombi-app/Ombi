#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: WebDriverSetup.cs
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
using System;

using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace PlexRequests.Automation
{

    public static class WebDriverSetup
    {
        private static IWebDriver _webDriver;
        
        public static IWebDriver SetUp(string url)
        {
            var driverService = FirefoxDriverService.CreateDefaultService("C:\\Tools\\WebDriver");
            driverService.FirefoxBinaryPath = @"C:\Program Files (x86)\Mozilla Firefox\firefox.exe";
            driverService.HideCommandPromptWindow = true;
            driverService.SuppressInitialDiagnosticInformation = true;
            _webDriver = new FirefoxDriver(driverService, new FirefoxOptions(), TimeSpan.FromSeconds(60));
            _webDriver.Navigate().GoToUrl(url);
            _webDriver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromMinutes(1));
            _webDriver.Manage().Window.Maximize();
            return _webDriver;
        }
    }

}