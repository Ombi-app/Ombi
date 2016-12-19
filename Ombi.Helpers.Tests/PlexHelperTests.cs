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

using System.Collections.Generic;
using NUnit.Framework;

namespace Ombi.Helpers.Tests
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

        [TestCaseSource(nameof(SeasonNumbers))]
        public int TitleToSeasonNumber(string title)
        {
            return PlexHelper.GetSeasonNumberFromTitle(title);
        }

        [TestCaseSource(nameof(MediaUrls))]
        public string GetPlexMediaUrlTest(string machineId, string mediaId)
        {
            return PlexHelper.GetPlexMediaUrl(machineId, mediaId);
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

        private static IEnumerable<TestCaseData> MediaUrls
        {
            get
            {
                yield return new TestCaseData("abcd","99").Returns("https://app.plex.tv/web/app#!/server/abcd/details/%2Flibrary%2Fmetadata%2F99").SetName("Test 1");
                yield return new TestCaseData("a54d1db669799308cd704b791f331eca6648b952", "51").Returns("https://app.plex.tv/web/app#!/server/a54d1db669799308cd704b791f331eca6648b952/details/%2Flibrary%2Fmetadata%2F51").SetName("Test 2");
            }
        }

        private static IEnumerable<TestCaseData> SeasonNumbers
        {
            get
            {
                yield return new TestCaseData("Season 1").Returns(1).SetName("Season 1");
                yield return new TestCaseData("Season 2").Returns(2).SetName("Season 2");
                yield return new TestCaseData("Season 3").Returns(3).SetName("Season 3");
                yield return new TestCaseData("Season 4").Returns(4).SetName("Season 4");
                yield return new TestCaseData("Season 5").Returns(5).SetName("Season 5");
                yield return new TestCaseData("Season 100").Returns(100).SetName("Season 100");
                yield return new TestCaseData("InvalidSeason").Returns(0).SetName("InvalidSeason");
                yield return new TestCaseData("Invalid Season with no number").Returns(0).SetName("Invalid Season with no number");
                yield return new TestCaseData("").Returns(0).SetName("Empty string");
                yield return new TestCaseData(null).Returns(0).SetName("Null string");
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