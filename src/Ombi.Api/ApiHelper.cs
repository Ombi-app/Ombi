#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: ApiHelper.cs
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

namespace Ombi.Api
{
    public static class ApiHelper
    {
        public static Uri ChangePath(this Uri uri, string path, params string[] args)
        {
            var builder = new UriBuilder(uri);
            
            if (args != null && args.Length > 0)
            {
                builder.Path = builder.Path + string.Format(path, args);
            }
            else
            {
                builder.Path += path;
            }
            return builder.Uri;
        }
        public static Uri ChangePath(this Uri uri, string path)
        {
            return ChangePath(uri, path, null);
        }

        public static Uri AddQueryParameter(this Uri uri, string parameter, string value)
        {
            if (string.IsNullOrEmpty(parameter) || string.IsNullOrEmpty(value)) return uri;
            var builder = new UriBuilder(uri);
            var startingTag = string.Empty;
            var hasQuery = false;
            if (string.IsNullOrEmpty(builder.Query))
            {
                startingTag = "?";
            }
            else
            {
                hasQuery = true;
                startingTag = builder.Query.Contains("?") ? "&" : "?";
            }

            value = Uri.EscapeDataString(value);
            
            builder.Query = hasQuery
                ? $"{builder.Query}{startingTag}{parameter}={value}"
                : $"{startingTag}{parameter}={value}";
            return builder.Uri;
        }
    }
}