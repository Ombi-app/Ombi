using System;
using NUnit.Framework;
using System.Collections.Generic;

namespace Ombi.Helpers.Tests
{
    [TestFixture]
    public class PlexHelperTests
    {

        [TestCaseSource(nameof(ProviderIdGuidData))]
        public string GetProviderIdFromPlexGuidTests(string guidInput, ProviderIdType type)
        {
            var result = PlexHelper.GetProviderIdFromPlexGuid(guidInput);

            switch (type)
            {
                case ProviderIdType.Imdb:
                    Assert.That(result.ImdbId, Is.Not.Null);
                    return result.ImdbId;
                case ProviderIdType.TvDb:
                    Assert.That(result.TheTvDb, Is.Not.Null);
                    return result.TheTvDb;
                case ProviderIdType.MovieDb:
                    Assert.That(result.TheMovieDb, Is.Not.Null);
                    return result.TheMovieDb;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static IEnumerable<TestCaseData> ProviderIdGuidData
        {
            get
            {
                yield return new TestCaseData("com.plexapp.agents.thetvdb://269586/2/8?lang=en", ProviderIdType.TvDb).Returns("269586").SetName("Regular TvDb Id");
                yield return new TestCaseData("com.plexapp.agents.themoviedb://390043?lang=en", ProviderIdType.MovieDb).Returns("390043").SetName("Regular MovieDb Id");
                yield return new TestCaseData("com.plexapp.agents.imdb://tt2543164?lang=en", ProviderIdType.Imdb).Returns("tt2543164").SetName("Regular Imdb Id");
                yield return new TestCaseData("com.plexapp.agents.agent47://tt2543456?lang=en", ProviderIdType.Imdb).Returns("tt2543456").SetName("Unknown IMDB agent");
                yield return new TestCaseData("com.plexapp.agents.agent47://456822/1/1?lang=en", ProviderIdType.TvDb).Returns("456822").SetName("Unknown TvDb agent");
                yield return new TestCaseData("com.plexapp.agents.agent47://456822/999/999?lang=en", ProviderIdType.TvDb).Returns("456822").SetName("Unknown TvDb agent, large episode and season");
                yield return new TestCaseData("com.plexapp.agents.xbmcnfotv://153021/2/1?lang=xn", ProviderIdType.TvDb).Returns("153021").SetName("xmbc agent, tv episode");
                yield return new TestCaseData("com.plexapp.agents.xbmcnfotv://153021?lang=xn", ProviderIdType.TvDb).Returns("153021").SetName("xmbc agent, tv show");
            }
        }

        public enum ProviderIdType
        {
            Imdb,
            TvDb,
            MovieDb
        }
    }
}