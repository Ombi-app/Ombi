#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: UriHelperTests.cs
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
using NUnit.Framework;

namespace PlexRequests.Helpers.Tests
{
    [TestFixture]
    public class UriHelperTests
    {
        [TestCaseSource(nameof(UriData))]
        public void CreateUri(string uri, Uri expected)
        { 
            var result = uri.ReturnUri();

            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(UriDataWithPort))]
        public void CreateUri(string uri, int port, Uri expected)
        {
            var result = uri.ReturnUri(port);

            Assert.That(result, Is.EqualTo(expected));
        }

        static readonly object[] UriData =
        {
            new object[] { "google.com", new Uri("http://google.com/"),  },
            new object[] { "http://google.com", new Uri("http://google.com/"),  },
            new object[] { "https://google.com", new Uri("https://google.com/"),  },
            new object[] { "192.168.1.1", new Uri("http://192.168.1.1")},
            new object[] { "0.0.0.0:5533", new Uri("http://0.0.0.0:5533")},
            new object[] {"www.google.com", new Uri("http://www.google.com/")},
            new object[] {"http://www.google.com/", new Uri("http://www.google.com/") },
            new object[] {"https://www.google.com", new Uri("https://www.google.com/") },
            new object[] {"www.google.com:443", new Uri("http://www.google.com:443/") },
            new object[] {"https://www.google.com:443", new Uri("https://www.google.com:443/") },
            new object[] {"http://www.google.com:443/id=2", new Uri("http://www.google.com:443/id=2") },
            new object[] {"www.google.com:4438/id=22", new Uri("http://www.google.com:4438/id=22") }
        };

        static readonly object[] UriDataWithPort =
        {
            new object[] {"www.google.com", 80, new Uri("http://www.google.com:80/"),  },
            new object[] {"www.google.com", 443, new Uri("http://www.google.com:443/") },
            new object[] {"http://www.google.com", 443, new Uri("http://www.google.com:443/") },
            new object[] {"https://www.google.com", 443, new Uri("https://www.google.com:443/") },
            new object[] {"http://www.google.com/id=2", 443, new Uri("http://www.google.com:443/id=2") },
            new object[] {"http://www.google.com/id=2", 443, new Uri("http://www.google.com:443/id=2") },
            new object[] {"https://www.google.com/id=2", 443, new Uri("https://www.google.com:443/id=2") },
        };
    }
}