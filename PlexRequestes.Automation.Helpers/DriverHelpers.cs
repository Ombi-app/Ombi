using System;

using OpenQA.Selenium;

namespace PlexRequestes.Automation.Helpers
{
    public static class DriverHelpers
    {
        public static bool Exists(this IWebElement element, bool preformAStringEmptyCheck)
        {
            try
            {
                var text = element.Text;
                if (preformAStringEmptyCheck)
                {
                    if (string.IsNullOrEmpty(text))
                    {
                        return false;
                    }
                }
            }
            catch (NoSuchElementException)
            {
                return false;
            }

            return true;
        }

        public static bool Exists(this IWebDriver driver, By locator, bool preformAStringEmptyCheck)
        {
            try
            {
                var element = driver.FindElement(locator);
                var text = element.Text;
                if (preformAStringEmptyCheck)
                {
                    if (string.IsNullOrEmpty(text))
                    {
                        return false;
                    }
                }
            }
            catch (NoSuchElementException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The exists.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>
        /// The <see cref="bool" />.
        /// </returns>
        public static bool Exists(this IWebElement element)
        {
            try
            {
                var text = element.Text;
                if (string.IsNullOrEmpty(text))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
