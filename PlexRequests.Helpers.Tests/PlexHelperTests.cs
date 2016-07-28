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
using System.Collections.Generic;

using NUnit.Framework;

namespace PlexRequests.Helpers.Tests
{
    [TestFixture]
    public class PlexHelperTests
    {
        [TestCaseSource(nameof(PlexGuids))]
        public string GetProviderId(string guid)
        {
            return PlexHelper.GetProviderIdFromPlexGuid(guid);
        }

        [TestCaseSource(nameof(PlexTvEpisodeGuids))]
        public List<string> GetEpisodeAndSeasons(string guid)
        {
            var ep = PlexHelper.GetSeasonsAndEpisodesFromPlexGuid(guid);
            var list = new List<string>
            {
                ep.ProviderId,
                ep.SeasonNumber.ToString(),
                ep.EpisodeNumber.ToString(),
            };

            return list;
        }


        private static IEnumerable<TestCaseData> PlexGuids
        {
            get
            {
                yield return new TestCaseData("com.plexapp.agents.thetvdb://269586/3/17?lang=en").Returns("269586");
                yield return new TestCaseData("com.plexapp.agents.imdb://tt3300542?lang=en").Returns("tt3300542");
                yield return new TestCaseData("com.plexapp.agents.thetvdb://71326/10/5?lang=en").Returns("71326");
                yield return new TestCaseData("local://3450").Returns("3450");
                yield return new TestCaseData("com.plexapp.agents.imdb://tt1179933?lang=en").Returns("tt1179933");
                yield return new TestCaseData("com.plexapp.agents.imdb://tt0284837?lang=en").Returns("tt0284837");
                yield return new TestCaseData("com.plexapp.agents.imdb://tt0076759?lang=en").Returns("tt0076759");
            }
        }

        private static IEnumerable<TestCaseData> PlexTvEpisodeGuids
        {
            get
            {
                yield return new TestCaseData("com.plexapp.agents.thetvdb://269586/2/8?lang=en").Returns(new List<string> { "269586","2","8" })
                    .SetName("Valid");
                yield return new TestCaseData("com.plexapp.agents.thetvdb://abc/112/388?lang=en").Returns(new List<string> { "abc", "112","388" })
                    .SetName("Valid Long");
            }
        }


    }
}