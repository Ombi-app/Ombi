#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: CacheKeys.cs
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
namespace PlexRequests.Core
{
    public class CacheKeys
    {
        public struct TimeFrameMinutes
        {
            public const int SchedulerCaching = 60;
        }

        public const string PlexLibaries = nameof(PlexLibaries);

        public const string TvDbToken = nameof(TvDbToken);

        public const string SonarrQualityProfiles = nameof(SonarrQualityProfiles);
        public const string SonarrQueued = nameof(SonarrQueued);

        public const string SickRageQualityProfiles = nameof(SickRageQualityProfiles);
        public const string SickRageQueued = nameof(SickRageQueued);

        public const string CouchPotatoQualityProfiles = nameof(CouchPotatoQualityProfiles);
        public const string CouchPotatoQueued = nameof(CouchPotatoQueued);

        public const string GetPlexRequestSettings = nameof(GetPlexRequestSettings);

        public const string LastestProductVersion = nameof(LastestProductVersion);
    }
}