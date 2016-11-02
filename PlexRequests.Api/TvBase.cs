#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: MovieBase.cs
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

using PlexRequests.Helpers;

namespace PlexRequests.Api
{
    public abstract class TvBase
    {
        private static readonly string Encrypted = "AVdhrVK6XX8anvrQgEyN/qNr9rk8ZPwy7/r1t5t5cKyUEzxcyk0L1v6dSxgE7hTCxvITUX2cWa6VlFMlTMgJWyuPZml7fN3csCHntgd/VGYro6VfNf24snZ/rQ3mf005";
        protected string ApiKey = StringCipher.Decrypt(Encrypted, "ApiKey");
        protected Uri Url = new Uri("https://api-beta.thetvdb.com/");
    }
}
