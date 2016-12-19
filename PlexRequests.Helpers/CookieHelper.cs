#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: CookieHelper.cs
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

namespace Ombi.Helpers
{
    public static class CookieHelper
    {
        private const string GaCookie = "_ga";

        /// <summary>
        /// Gets the analytic client identifier.
        /// <para>Example: Value = "GA1.1.306549087.1464005217"</para>
        /// </summary>
        /// <param name="cookies">The cookies.</param>
        /// <returns></returns>
        public static string GetAnalyticClientId(IDictionary<string, string> cookies)
        {
            string outString;

            if (cookies.TryGetValue(GaCookie, out outString))
            {
                var split = outString.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                return split.Length < 4 
                    ? string.Empty 
                    : $"{split[2]}.{split[3]}";
            }
            return string.Empty;
        }
    }
}