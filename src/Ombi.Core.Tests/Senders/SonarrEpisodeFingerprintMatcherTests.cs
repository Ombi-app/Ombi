using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Ombi.Api.External.ExternalApis.Sonarr.Models;
using Ombi.Core.Senders;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Tests.Senders
{
    [TestFixture]
    public class SonarrEpisodeFingerprintMatcherTests
    {
        [Test]
        public void StandaloneSeason_MapsToAnthologySeason_EvenWhenSameNumberExists()
        {
            var source = BuildSeason(1, LizzieTitles);
            var sonarrEpisodes = BuildEpisodes(1, DahmerTitles)
                .Concat(BuildEpisodes(4, LizzieTitles))
                .ToList();

            var result = SonarrEpisodeFingerprintMatcher.FindSingleSeasonMatch(source, sonarrEpisodes);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.SourceSeasonNumber, Is.EqualTo(1));
            Assert.That(result.SonarrSeasonNumber, Is.EqualTo(4));
            Assert.That(SonarrEpisodeFingerprintMatcher.HasConflictingExactSeason(source, sonarrEpisodes), Is.True);
        }

        [Test]
        public void NormalSeason_KeepsExactSeason_WhenEpisodeFingerprintMatches()
        {
            var source = BuildSeason(1, DahmerTitles);
            var sonarrEpisodes = BuildEpisodes(1, DahmerTitles)
                .Concat(BuildEpisodes(4, LizzieTitles))
                .ToList();

            var result = SonarrEpisodeFingerprintMatcher.FindSingleSeasonMatch(source, sonarrEpisodes);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.SonarrSeasonNumber, Is.EqualTo(1));
            Assert.That(SonarrEpisodeFingerprintMatcher.HasConflictingExactSeason(source, sonarrEpisodes), Is.False);
        }

        [Test]
        public void AmbiguousFingerprint_DoesNotGuess()
        {
            var source = BuildSeason(1, LizzieTitles.Take(3).ToArray());
            var sonarrEpisodes = BuildEpisodes(3, LizzieTitles.Take(3).ToArray())
                .Concat(BuildEpisodes(4, LizzieTitles.Take(3).ToArray()))
                .ToList();

            var result = SonarrEpisodeFingerprintMatcher.FindSingleSeasonMatch(source, sonarrEpisodes);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void TooSmallFingerprint_WithConflictingExactSeason_IsDetectedAsUnsafe()
        {
            var source = BuildSeason(1, LizzieTitles.Take(2).ToArray());
            var sonarrEpisodes = BuildEpisodes(1, DahmerTitles)
                .Concat(BuildEpisodes(4, LizzieTitles))
                .ToList();

            var result = SonarrEpisodeFingerprintMatcher.FindSingleSeasonMatch(source, sonarrEpisodes);

            Assert.That(result, Is.Null);
            Assert.That(SonarrEpisodeFingerprintMatcher.HasConflictingExactSeason(source, sonarrEpisodes), Is.True);
        }

        [Test]
        public void ExactSeason_WithProviderTitleVariantsOnAvailableEpisodes_IsSafeWhenOutstandingEpisodeMatches()
        {
            var source = BuildSeason(2, BadGuysTmdbTitles);
            foreach (var episode in source.Episodes)
            {
                episode.Requested = true;
                episode.Approved = true;
                episode.Available = true;
            }
            source.Episodes.Single(x => x.EpisodeNumber == 7).Available = false;

            var sonarrEpisodes = BuildEpisodes(2, BadGuysSonarrTitles).ToList();

            var result = SonarrEpisodeFingerprintMatcher.FindSingleSeasonMatch(source, sonarrEpisodes);

            Assert.That(result, Is.Null, "Provider title variants should not masquerade as a full fingerprint match");
            Assert.That(SonarrEpisodeFingerprintMatcher.HasConflictingExactSeason(source, sonarrEpisodes), Is.False);
        }

        [Test]
        public void ExactSeason_WhenOutstandingEpisodeTitleDiffers_RemainsUnsafe()
        {
            var source = BuildSeason(2, BadGuysTmdbTitles);
            foreach (var episode in source.Episodes)
            {
                episode.Requested = true;
                episode.Approved = true;
                episode.Available = true;
            }
            source.Episodes.Single(x => x.EpisodeNumber == 9).Available = false;

            var sonarrEpisodes = BuildEpisodes(2, BadGuysSonarrTitles).ToList();

            Assert.That(SonarrEpisodeFingerprintMatcher.HasConflictingExactSeason(source, sonarrEpisodes), Is.True);
        }

        [Test]
        public void ExactSeason_WithTooManyProviderTitleDifferences_RemainsUnsafe()
        {
            var source = BuildSeason(2, BadGuysTmdbTitles);
            foreach (var episode in source.Episodes)
            {
                episode.Requested = true;
                episode.Approved = true;
                episode.Available = true;
            }
            source.Episodes.Single(x => x.EpisodeNumber == 7).Available = false;

            var weakMatchTitles = BadGuysSonarrTitles.ToArray();
            weakMatchTitles[5] = "Completely Different Six";
            var sonarrEpisodes = BuildEpisodes(2, weakMatchTitles).ToList();

            Assert.That(SonarrEpisodeFingerprintMatcher.HasConflictingExactSeason(source, sonarrEpisodes), Is.True);
        }

        [Test]
        public void ExactSeason_WithDifferentEpisodeStructure_RemainsUnsafe()
        {
            var source = BuildSeason(2, BadGuysTmdbTitles);
            foreach (var episode in source.Episodes)
            {
                episode.Requested = true;
                episode.Approved = true;
                episode.Available = true;
            }
            source.Episodes.Single(x => x.EpisodeNumber == 7).Available = false;

            var sonarrEpisodes = BuildEpisodes(2, BadGuysSonarrTitles)
                .Concat(new[]
                {
                    new Episode
                    {
                        id = 211,
                        seasonNumber = 2,
                        episodeNumber = 11,
                        title = "Unexpected Episode"
                    }
                })
                .ToList();

            Assert.That(SonarrEpisodeFingerprintMatcher.HasConflictingExactSeason(source, sonarrEpisodes), Is.True);
        }

        [Test]
        public void ExactSeason_WithLocalizedTitlesAndStrongAirDates_IsSafe()
        {
            var source = BuildSeason(1, MetalFamilyTmdbTitles, Dates(
                "2018-09-13", "2018-09-26", "2018-10-11", "2018-10-24", "2018-11-08",
                "2019-01-19", "2019-03-15", "2019-05-28", "2019-11-01", "2019-11-02"));
            var sonarrEpisodes = BuildEpisodes(1, MetalFamilySonarrTitles, Dates(
                "2018-09-13", "2018-09-26", "2018-10-11", "2018-10-25", "2018-11-08",
                "2019-01-19", "2019-03-15", "2019-05-28", "2019-11-01", "2020-05-13")).ToList();

            var result = SonarrEpisodeFingerprintMatcher.FindSingleSeasonMatch(source, sonarrEpisodes);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.SonarrSeasonNumber, Is.EqualTo(1));
            Assert.That(SonarrEpisodeFingerprintMatcher.HasConflictingExactSeason(source, sonarrEpisodes), Is.False);
        }

        [Test]
        public void ExactSeason_WithGenericProviderTitlesAndExactAirDates_IsSafe()
        {
            var source = BuildSeason(2, AimHighTmdbTitles, Dates(
                "2013-10-01", "2013-10-08", "2013-10-15", "2013-10-22", "2013-10-29",
                "2013-11-05", "2013-11-12", "2013-11-19", "2013-11-26", "2013-12-02"));
            var sonarrEpisodes = BuildEpisodes(2, AimHighSonarrTitles, Dates(
                "2013-10-01", "2013-10-08", "2013-10-15", "2013-10-22", "2013-10-29",
                "2013-11-05", "2013-11-12", "2013-11-19", "2013-11-26", "2013-12-02")).ToList();

            var result = SonarrEpisodeFingerprintMatcher.FindSingleSeasonMatch(source, sonarrEpisodes);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.SonarrSeasonNumber, Is.EqualTo(2));
            Assert.That(SonarrEpisodeFingerprintMatcher.HasConflictingExactSeason(source, sonarrEpisodes), Is.False);
        }

        [Test]
        public void CrossSeason_WithStrongAirDateFingerprint_MapsToUniqueSonarrSeason()
        {
            var source = BuildSeason(1, CanadasGotTalentTmdbTitles, Dates(
                "2022-03-22", "2022-03-29", "2022-04-05", "2022-04-12", "2022-04-19",
                "2022-04-26", "2022-05-03", "2022-05-10", "2022-05-17"));

            var oldSeasonTitles = Enumerable.Range(1, 22).Select(x => $"2012 Episode {x}").ToArray();
            var oldSeasonDates = Enumerable.Range(0, 22)
                .Select(x => new DateTime(2012, 3, 4).AddDays(x * 3))
                .ToArray();
            var sonarrEpisodes = BuildEpisodes(1, oldSeasonTitles, oldSeasonDates)
                .Concat(BuildEpisodes(2, CanadasGotTalentSonarrTitles, Dates(
                    "2022-03-22", "2022-03-29", "2022-04-05", "2022-04-12", "2022-04-19",
                    "2022-04-26", "2022-05-03", "2022-05-10", "2022-05-17")))
                .ToList();

            var result = SonarrEpisodeFingerprintMatcher.FindSingleSeasonMatch(source, sonarrEpisodes);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.SourceSeasonNumber, Is.EqualTo(1));
            Assert.That(result.SonarrSeasonNumber, Is.EqualTo(2));
            Assert.That(SonarrEpisodeFingerprintMatcher.HasConflictingExactSeason(source, sonarrEpisodes), Is.True);
        }

        [Test]
        public void SparseAirDates_DoNotOverrideUnsafeTitleMismatch()
        {
            var source = BuildSeason(1, DeaTmdbTitles, Dates(
                "2009-01-07", "2009-01-14", "2009-01-21", "2009-01-28", "2009-02-04", "2009-02-11"));
            var sonarrEpisodes = BuildEpisodes(1, Enumerable.Repeat("TBA", 6).ToArray(), new DateTime?[]
                {
                    new DateTime(2011, 8, 25),
                    new DateTime(2009, 1, 14),
                    new DateTime(2009, 1, 21),
                    null,
                    null,
                    null
                })
                .Concat(BuildEpisodes(2, Enumerable.Repeat("TBA", 9).ToArray(), new DateTime?[9]))
                .ToList();

            var result = SonarrEpisodeFingerprintMatcher.FindSingleSeasonMatch(source, sonarrEpisodes);

            Assert.That(result, Is.Null);
            Assert.That(SonarrEpisodeFingerprintMatcher.HasConflictingExactSeason(source, sonarrEpisodes), Is.True);
        }

        [Test]
        public void AirDateFingerprint_WithDifferentEpisodeStructure_DoesNotMatch()
        {
            var source = BuildSeason(1, AimHighTmdbTitles, Dates(
                "2013-10-01", "2013-10-08", "2013-10-15", "2013-10-22", "2013-10-29",
                "2013-11-05", "2013-11-12", "2013-11-19", "2013-11-26", "2013-12-02"));
            var sonarrEpisodes = BuildEpisodes(1, AimHighSonarrTitles, Dates(
                    "2013-10-01", "2013-10-08", "2013-10-15", "2013-10-22", "2013-10-29",
                    "2013-11-05", "2013-11-12", "2013-11-19", "2013-11-26", "2013-12-02"))
                .Concat(new[]
                {
                    new Episode
                    {
                        id = 111,
                        seasonNumber = 1,
                        episodeNumber = 11,
                        title = "Unexpected Episode",
                        airDate = "2013-12-09"
                    }
                })
                .ToList();

            var result = SonarrEpisodeFingerprintMatcher.FindSingleSeasonMatch(source, sonarrEpisodes);

            Assert.That(result, Is.Null);
            Assert.That(SonarrEpisodeFingerprintMatcher.HasConflictingExactSeason(source, sonarrEpisodes), Is.True);
        }

        [Test]
        public void AirDateFingerprint_BelowNinetyPercentAgreement_RemainsUnsafe()
        {
            var source = BuildSeason(1, AimHighTmdbTitles, Dates(
                "2013-10-01", "2013-10-08", "2013-10-15", "2013-10-22", "2013-10-29",
                "2013-11-05", "2013-11-12", "2013-11-19", "2013-11-26", "2013-12-02"));
            var sonarrEpisodes = BuildEpisodes(1, AimHighSonarrTitles, Dates(
                "2013-10-01", "2013-10-08", "2013-10-15", "2013-10-22", "2013-10-29",
                "2013-11-05", "2013-11-12", "2013-11-19", "2014-01-01", "2014-01-08")).ToList();

            var result = SonarrEpisodeFingerprintMatcher.FindSingleSeasonMatch(source, sonarrEpisodes);

            Assert.That(result, Is.Null);
            Assert.That(SonarrEpisodeFingerprintMatcher.HasConflictingExactSeason(source, sonarrEpisodes), Is.True);
        }

        [Test]
        public void AirDateFingerprint_WhenMultipleSeasonsMatch_DoesNotGuess()
        {
            var source = BuildSeason(1, AimHighTmdbTitles, Dates(
                "2013-10-01", "2013-10-08", "2013-10-15", "2013-10-22", "2013-10-29",
                "2013-11-05", "2013-11-12", "2013-11-19", "2013-11-26", "2013-12-02"));
            var dates = Dates(
                "2013-10-01", "2013-10-08", "2013-10-15", "2013-10-22", "2013-10-29",
                "2013-11-05", "2013-11-12", "2013-11-19", "2013-11-26", "2013-12-02");
            var sonarrEpisodes = BuildEpisodes(1, AimHighSonarrTitles, dates)
                .Concat(BuildEpisodes(3, AimHighSonarrTitles, dates))
                .ToList();

            var result = SonarrEpisodeFingerprintMatcher.FindSingleSeasonMatch(source, sonarrEpisodes);

            Assert.That(result, Is.Null);
            Assert.That(SonarrEpisodeFingerprintMatcher.HasConflictingExactSeason(source, sonarrEpisodes), Is.True);
        }

        [Test]
        public void SeriesIdentity_TotalDramaAction_StrongStructuralTitleFingerprintMapsToConsolidatedSeason()
        {
            var source = BuildSeason(1, TotalDramaActionStandaloneTitles);
            var sonarrEpisodes = BuildEpisodes(2, TotalDramaActionConsolidatedTitles)
                .Concat(BuildEpisodes(3, Enumerable.Range(1, 10).Select(x => $"Other Episode {x}").ToArray()))
                .ToList();

            var normalResult = SonarrEpisodeFingerprintMatcher.FindSingleSeasonMatch(source, sonarrEpisodes);
            var identityResult = SonarrEpisodeFingerprintMatcher.FindSingleSeriesIdentityMatch(source, sonarrEpisodes);

            Assert.That(normalResult, Is.Not.Null);
            Assert.That(normalResult.SourceSeasonNumber, Is.EqualTo(1));
            Assert.That(normalResult.SonarrSeasonNumber, Is.EqualTo(2));
            Assert.That(identityResult, Is.Not.Null);
            Assert.That(identityResult.SonarrSeasonNumber, Is.EqualTo(2));
        }

        [Test]
        public void SeriesIdentity_PahkitewShiftedIntoSecondHalfOfSeason_DoesNotInferEpisodeOffset()
        {
            var source = BuildSeason(1, PahkitewTitles);
            var firstHalf = Enumerable.Range(1, 13).Select(x => $"All Stars {x}").ToArray();
            var combinedSeason = firstHalf.Concat(PahkitewTitles).ToArray();
            var sonarrEpisodes = BuildEpisodes(5, combinedSeason).ToList();

            var result = SonarrEpisodeFingerprintMatcher.FindSingleSeriesIdentityMatch(source, sonarrEpisodes);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void SeriesIdentity_WhenMultipleSeasonsHaveStrongStructuralTitleFingerprint_DoesNotGuess()
        {
            var source = BuildSeason(1, TotalDramaActionStandaloneTitles);
            var sonarrEpisodes = BuildEpisodes(2, TotalDramaActionConsolidatedTitles)
                .Concat(BuildEpisodes(7, TotalDramaActionConsolidatedTitles))
                .ToList();

            var result = SonarrEpisodeFingerprintMatcher.FindSingleSeriesIdentityMatch(source, sonarrEpisodes);

            Assert.That(result, Is.Null);
        }

        private static SeasonRequests BuildSeason(int seasonNumber, IReadOnlyList<string> titles)
        {
            return BuildSeason(seasonNumber, titles, null);
        }

        private static SeasonRequests BuildSeason(
            int seasonNumber,
            IReadOnlyList<string> titles,
            IReadOnlyList<DateTime> airDates)
        {
            return new SeasonRequests
            {
                SeasonNumber = seasonNumber,
                Episodes = titles.Select((title, index) => new EpisodeRequests
                {
                    EpisodeNumber = index + 1,
                    Title = title,
                    AirDate = airDates != null && index < airDates.Count
                        ? airDates[index]
                        : DateTime.MinValue
                }).ToList()
            };
        }

        private static IEnumerable<Episode> BuildEpisodes(int seasonNumber, IReadOnlyList<string> titles)
        {
            return BuildEpisodes(seasonNumber, titles, (IReadOnlyList<DateTime>)null);
        }

        private static IEnumerable<Episode> BuildEpisodes(
            int seasonNumber,
            IReadOnlyList<string> titles,
            IReadOnlyList<DateTime> airDates)
        {
            return BuildEpisodes(
                seasonNumber,
                titles,
                airDates?.Select(x => (DateTime?)x).ToArray());
        }

        private static IEnumerable<Episode> BuildEpisodes(
            int seasonNumber,
            IReadOnlyList<string> titles,
            IReadOnlyList<DateTime?> airDates)
        {
            return titles.Select((title, index) => new Episode
            {
                id = (seasonNumber * 100) + index + 1,
                seasonNumber = seasonNumber,
                episodeNumber = index + 1,
                title = title,
                airDate = airDates != null && index < airDates.Count && airDates[index].HasValue
                    ? airDates[index].Value.ToString("yyyy-MM-dd")
                    : null
            });
        }

        private static DateTime[] Dates(params string[] dates)
        {
            return dates.Select(x => DateTime.ParseExact(x, "yyyy-MM-dd", CultureInfo.InvariantCulture)).ToArray();
        }

        private static readonly string[] DahmerTitles =
        {
            "Episode One",
            "Please Don't Go",
            "Doin' a Dahmer",
            "The Good Boy Box",
            "Blood on Their Hands",
            "Silenced",
            "Cassandra",
            "Lionel",
            "The Bogeyman",
            "God of Forgiveness, God of Vengeance"
        };

        private static readonly string[] LizzieTitles =
        {
            "Bloodbath",
            "Strong Kitty",
            "Whack Job!",
            "R.I.P (Rest in Pestilence) Abby Borden",
            "41",
            "Bed and Breakfast",
            "The Trial of the Century",
            "Carnival"
        };

        private static readonly string[] BadGuysTmdbTitles =
        {
            "The Heist-Over",
            "The Con Test",
            "Natural Heistory",
            "A Nice Day for a Bad Wedding",
            "Me Mentor Mori",
            "Double Jeopardy",
            "I, Webs",
            "Bad Actors",
            "Fear the Ripper (1)",
            "Fear the Ripper (2)"
        };

        private static readonly string[] BadGuysSonarrTitles =
        {
            "The Heist-Over",
            "The Con Test",
            "Natural Heistory",
            "A Nice Day for a Bad Wedding",
            "Me Mentor Mori",
            "Double Jeopardy",
            "I, Webs",
            "Bad Actors",
            "Fear the Ripper",
            "Fear the Ripper Pt. 2"
        };


        private static readonly string[] TotalDramaActionStandaloneTitles =
        {
            "Monster Cash!",
            "Alien Resurr-eggtion",
            "Riot on Set",
            "Beach Blanket Bogus",
            "3:10 To Crazytown",
            "TDA Aftermath: I",
            "The Chefshank Redemption",
            "One Flu Over The Cuckoos",
            "The Sand Witch Project",
            "Masters of Disasters"
        };

        private static readonly string[] TotalDramaActionConsolidatedTitles =
        {
            "Monster Cash",
            "Alien Resurr-eggtion",
            "Riot on Set",
            "Beach Blanket Bogus",
            "3:10 to Crazytown",
            "TDA Aftermath I: Trent's Descent",
            "The Chefshank Redemption",
            "One Flu Over the Cuckoos",
            "The Sand Witch Project",
            "Masters of Disasters"
        };

        private static readonly string[] PahkitewTitles =
        {
            "So, Uh, This Is My Team?",
            "I Love You, Grease Pig!",
            "Twinning Isn't Everything",
            "I Love You, I Love You Knots",
            "A Blast from the Past",
            "Mo Monkey Mo Problems",
            "This Is the Pits!",
            "Three Zones and a Baby",
            "Hurl and Go Seek",
            "Scarlett Fever",
            "Sky Fall",
            "Pahk'd With Talent",
            "Lies, Cries and One Big Prize"
        };

        private static readonly string[] MetalFamilyTmdbTitles =
        {
            "Father to school",
            "protector",
            "stasik contra",
            "poker",
            "BOX 37",
            "QUEST",
            "Do you play the guitar?",
            "Morning Beavers",
            "Pitch Perfect",
            "GLAM"
        };

        private static readonly string[] MetalFamilySonarrTitles =
        {
            "Father to school",
            "Intercessor",
            "Stasik against",
            "Poker",
            "Box 37",
            "Quest",
            "Do you play guitar?",
            "Day beavers",
            "Perfect hearing",
            "Glam"
        };

        private static readonly string[] AimHighTmdbTitles = Enumerable.Range(1, 10)
            .Select(x => $"Episode {x}")
            .ToArray();

        private static readonly string[] AimHighSonarrTitles = Enumerable.Range(1, 10)
            .Select(x => $"Episode #2.{x}")
            .ToArray();

        private static readonly string[] CanadasGotTalentTmdbTitles =
        {
            "Auditions 1",
            "Auditions 2",
            "Auditions 3",
            "Auditions 4",
            "Auditions 5",
            "Auditions 6",
            "Semi Finals 1",
            "Semi Finals 2",
            "Final"
        };

        private static readonly string[] CanadasGotTalentSonarrTitles =
        {
            "Auditions 1",
            "Auditions 2",
            "Auditions 3",
            "Auditions 4",
            "Auditions 5",
            "Auditions 6",
            "Semi-Finals 1",
            "Semi-Finals 2",
            "Finale"
        };

        private static readonly string[] DeaTmdbTitles = Enumerable.Range(1, 6)
            .Select(x => $"Episode {x}")
            .ToArray();
    }
}
