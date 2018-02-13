using System.Collections.Generic;
using NUnit.Framework;
using Ombi.Helpers;

namespace Ombi.Tests
{
    [TestFixture]
    public class PosterPathHelperTests
    {

        [TestCaseSource(nameof(TestData))]
        public string PostPathTest(string posterPath)
        {
            return PosterPathHelper.FixPosterPath(posterPath);
        }

        private static IEnumerable<TestCaseData> TestData
        {
            get
            {
                yield return new TestCaseData("https://image.tmdb.org/t/p/w150/fJAvGOitU8y53ByeHnM4avtKFaG.jpg").Returns("fJAvGOitU8y53ByeHnM4avtKFaG.jpg").SetName("Full tmdb poster path returns last part");
                yield return new TestCaseData("https://image.tmdb.org/t/p/w300/https://image.tmdb.org/t/p/w300//fJAvGOitU8y53ByeHnM4avtKFaG.jpg").Returns("fJAvGOitU8y53ByeHnM4avtKFaG.jpg").SetName("Double tmdb poster path returns last part");
            }
        }
    }
}