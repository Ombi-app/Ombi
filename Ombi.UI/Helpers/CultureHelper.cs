#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: CultureHelper.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Ombi.UI.Helpers
{
    public class CultureHelper
    {
        private static readonly List<string> ValidCultures = new List<string> { "en-US", "de-DE", "fr-FR", "es-ES", "de", "en", "fr", "es","da","sv","it","nl" };

        private static readonly List<string> ImplimentedCultures = new List<string> {
        "en-US",
        "en",
        "de",
        "fr",
        "es",
        "da",
        "sv",
        "it",
        "nl"
    };

        /// <summary>
        /// Returns true if the language is a right-to-left language.
        /// </summary>
        public static bool IsRighToLeft()
        {
            return Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft;
        }

        /// <summary>
        /// Returns a valid culture name based on "name" parameter. If "name" is not valid, it returns the default culture "en-US"
        /// </summary>
        /// <param name='name'> Culture's name (e.g. en-US) </param>
        public static string GetImplementedCulture(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return GetDefaultCulture();
            }

            // Validate the culture
            if (!ValidCultures.Any(c => c.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
            {
                return GetDefaultCulture();
            }

            if (ImplimentedCultures.Any(c => c.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
            {
                return name; // We do have the culture, lets go with it
            }

            // Find a close match. 
            var match = GetNeutralCulture(name);

            foreach (var c in ImplimentedCultures.Where(c => c.StartsWith(match, StringComparison.InvariantCultureIgnoreCase)))
            {
                return c;
            }

            return GetDefaultCulture(); // return Default culture since we cannot find anything
        }

        /// <summary>
        /// Returns default culture name which is the first name declared (e.g. en-US)
        /// </summary>
        /// <returns> Culture string e.g. en-US </returns>
        public static string GetDefaultCulture()
        {
            return ImplimentedCultures[0]; // return first culture
        }

        public static string GetCurrentCulture()
        {
            return Thread.CurrentThread.CurrentCulture.Name;
        }

        public static string GetCurrentNeutralCulture()
        {
            return GetNeutralCulture(Thread.CurrentThread.CurrentCulture.Name);
        }

        public static string GetNeutralCulture(string name)
        {
            if (!name.Contains("-")) return name;

            return name.Split('-')[0]; // Read first part only
        }
    }
}