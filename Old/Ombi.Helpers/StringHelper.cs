#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: StringHelper.cs
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
using System.Text;
using System.Text.RegularExpressions;

namespace Ombi.Helpers
{
    public static class StringHelper
    {
        public static string FirstCharToUpper(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var firstUpper = char.ToUpper(input[0]);
            return firstUpper + string.Join("", input.Skip(1));
        }

        public static string ToCamelCaseWords(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            return Regex.Replace(input.FirstCharToUpper(), "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
        }

        public static string AddPrefix(this string[] values, string prefix, string separator)
        {
            var sb = new StringBuilder();
            var len = values.Length;
            for (var i = 0; i < len; i++)
            {
                sb.Append(prefix).Append(values[i]);

                // If it's not the last item in the collection, then add a separator
                if (i < len - 1)
                {
                    sb.Append(separator);
                }
            }
            return sb.ToString();
        }

        public static IEnumerable<string> SplitEmailsByDelimiter(this string input, char delimiter)
        {
            if (string.IsNullOrEmpty(input))
            {
                yield return string.Empty;
            }
            var startIndex = 0;
            var delimiterIndex = 0;

            while (delimiterIndex >= 0)
            {
                if (input != null)
                {
                    delimiterIndex = input.IndexOf(delimiter, startIndex);
                    var substring = input;
                    if (delimiterIndex > 0)
                    {
                        substring = input.Substring(0, delimiterIndex).Trim();
                    }

                    if (!substring.Contains("\"") || substring.IndexOf("\"", StringComparison.Ordinal) != substring.LastIndexOf("\"", StringComparison.Ordinal))
                    {
                        yield return substring;
                        input = input.Substring(delimiterIndex + 1).Trim();
                        startIndex = 0;
                    }
                    else
                    {
                        startIndex = delimiterIndex + 1;
                    }
                }
            }
        }
    }
}