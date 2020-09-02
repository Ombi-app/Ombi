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
                yield return new TestCaseData("tmdb://610201", ProviderIdType.MovieDb).Returns("610201").SetName("Themoviedb new plex format");
            }
        }

        [TestCaseSource(nameof(ProviderIdGuidDataV2))]
        public void GetProviderIdsFromMetadataTests(string guidInput, ProviderId expected)
        {
            var param = guidInput.Split('|', StringSplitOptions.RemoveEmptyEntries);
            var result = PlexHelper.GetProviderIdsFromMetadata(param);

            Assert.AreEqual(expected.ImdbId, result.ImdbId);
            Assert.AreEqual(expected.TheMovieDb, result.TheMovieDb);
            Assert.AreEqual(expected.TheTvDb, result.TheTvDb);
        }

        public static IEnumerable<TestCaseData> ProviderIdGuidDataV2
        {
            get
            {
                yield return new TestCaseData("plex://movie/5e1632df2d4d84003e48e54e|imdb://tt9178402|tmdb://610201", new ProviderId { ImdbId = "tt9178402", TheMovieDb = "610201" }).SetName("V2 Regular Plex Id");
                yield return new TestCaseData("plex://movie/5d7768253c3c2a001fbcab72|imdb://tt0119567|tmdb://330", new ProviderId { ImdbId = "tt0119567", TheMovieDb = "330" }).SetName("V2 Regular Plex Id Another");
                yield return new TestCaseData("plex://movie/5d7768253c3c2a001fbcab72|imdb://tt0119567", new ProviderId { ImdbId = "tt0119567" }).SetName("V2 Regular Plex Id Single Imdb");
                yield return new TestCaseData("plex://movie/5d7768253c3c2a001fbcab72|tmdb://330", new ProviderId { TheMovieDb = "330" }).SetName("V2 Regular Plex Id Single Tmdb");
                yield return new TestCaseData("com.plexapp.agents.thetvdb://269586/2/8?lang=en", new ProviderId { TheTvDb = "269586" }).SetName("V2 Regular TvDb Id");
                yield return new TestCaseData("com.plexapp.agents.themoviedb://390043?lang=en", new ProviderId { TheMovieDb = "390043" }).SetName("V2 Regular MovieDb Id");
                yield return new TestCaseData("com.plexapp.agents.imdb://tt2543164?lang=en", new ProviderId { ImdbId = "tt2543164" }).SetName("V2 Regular Imdb Id");
                yield return new TestCaseData("com.plexapp.agents.agent47://tt2543456?lang=en", new ProviderId { ImdbId = "tt2543456" }).SetName("V2 Unknown IMDB agent");
                yield return new TestCaseData("com.plexapp.agents.agent47://456822/1/1?lang=en", new ProviderId { TheTvDb = "456822" }).SetName("V2 Unknown TvDb agent");
                yield return new TestCaseData("com.plexapp.agents.agent47://456822/999/999?lang=en", new ProviderId { TheTvDb = "456822" }).SetName("V2 Unknown TvDb agent, large episode and season");
                yield return new TestCaseData("com.plexapp.agents.xbmcnfotv://153021/2/1?lang=xn", new ProviderId { TheTvDb = "153021" }).SetName("V2 xmbc agent, tv episode");
                yield return new TestCaseData("com.plexapp.agents.xbmcnfotv://153021?lang=xn", new ProviderId { TheTvDb = "153021" }).SetName("V2 xmbc agent, tv show");
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