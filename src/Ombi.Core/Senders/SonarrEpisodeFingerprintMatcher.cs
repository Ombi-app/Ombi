using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ombi.Api.External.ExternalApis.Sonarr.Models;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Senders
{
    /// <summary>
    /// Resolves a TMDB-style requested season to the corresponding Sonarr season when providers
    /// disagree about whether an anthology season is a standalone show. Remaps are intentionally
    /// conservative: title fingerprints remain the primary signal, while strong episode-number and
    /// air-date fingerprints provide a secondary signal when providers localize or rename episode
    /// titles. Exact-number seasons may also tolerate a small number of title variants when the
    /// episode structure strongly agrees and every episode Ombi still needs matches exactly.
    /// </summary>
    public static class SonarrEpisodeFingerprintMatcher
    {
        private const int MinimumEpisodeFingerprintSize = 3;
        private const int MinimumComparableAirDatePercentage = 80;
        private const int MinimumMatchingAirDatePercentage = 90;
        private const int AirDateToleranceDays = 1;

        public static SonarrSeasonFingerprintMatch FindSingleSeasonMatch(
            SeasonRequests sourceSeason,
            IEnumerable<Episode> sonarrEpisodes)
        {
            var episodes = sonarrEpisodes?
                .Where(x => x != null)
                .ToList() ?? new List<Episode>();

            if (episodes.Count == 0)
            {
                return null;
            }

            // Preserve the existing title fingerprint as the primary signal. It is precise when
            // providers agree on episode names and already handles anthology/standalone remaps.
            var fingerprint = GetFingerprint(sourceSeason);
            if (fingerprint.Count >= MinimumEpisodeFingerprintSize)
            {
                // Use the longest title as an anchor to keep the candidate set small. The final
                // decision still requires every requested episode title/number to match.
                var anchor = fingerprint
                    .OrderByDescending(x => x.NormalizedTitle.Length)
                    .First();

                var candidateSeasons = episodes
                    .Where(x => x.episodeNumber == anchor.EpisodeNumber &&
                                TitlesMatch(x.title, anchor.NormalizedTitle))
                    .Select(x => x.seasonNumber)
                    .Distinct()
                    .ToList();

                var titleMatches = new List<int>();
                foreach (var candidateSeason in candidateSeasons)
                {
                    var candidateByNumber = episodes
                        .Where(x => x.seasonNumber == candidateSeason)
                        .GroupBy(x => x.episodeNumber)
                        .ToDictionary(x => x.Key, x => x.First().title);

                    var fullFingerprintMatches = fingerprint.All(expected =>
                        candidateByNumber.TryGetValue(expected.EpisodeNumber, out var actualTitle) &&
                        TitlesMatch(actualTitle, expected.NormalizedTitle));

                    if (fullFingerprintMatches)
                    {
                        titleMatches.Add(candidateSeason);
                    }
                }

                if (titleMatches.Count == 1)
                {
                    return new SonarrSeasonFingerprintMatch
                    {
                        SourceSeasonNumber = sourceSeason.SeasonNumber,
                        SonarrSeasonNumber = titleMatches[0]
                    };
                }
            }

            // A standalone provider show can also correspond to a differently-numbered season
            // in a consolidated Sonarr parent while a minority of display titles differ. Accept
            // this only for a unique cross-season candidate with the exact episode-number set and
            // at least 80% normalized title agreement. Exact-number seasons keep the stricter
            // outstanding-episode safeguards in HasConflictingExactSeason().
            var structuralTitleMatches = FindStrongStructuralTitleMatches(sourceSeason, episodes);
            if (structuralTitleMatches.Count == 1 &&
                structuralTitleMatches[0] != sourceSeason.SeasonNumber)
            {
                return new SonarrSeasonFingerprintMatch
                {
                    SourceSeasonNumber = sourceSeason.SeasonNumber,
                    SonarrSeasonNumber = structuralTitleMatches[0]
                };
            }

            // Providers can substantially rename/localize episode titles while still agreeing on
            // the season structure and original air dates. Fall back to a strict date fingerprint:
            // the complete episode-number set must match, at least 80% of the season must have
            // comparable dates, and at least 90% of those dates must agree within one day. A
            // cross-season remap is accepted only when exactly one Sonarr season satisfies it.
            var airDateMatches = FindAirDateFingerprintMatches(sourceSeason, episodes);
            if (airDateMatches.Count != 1)
            {
                return null;
            }

            return new SonarrSeasonFingerprintMatch
            {
                SourceSeasonNumber = sourceSeason.SeasonNumber,
                SonarrSeasonNumber = airDateMatches[0]
            };
        }


        /// <summary>
        /// Finds one season match strong enough to identify a consolidated Sonarr parent when
        /// provider IDs and exact titles cannot. This is intentionally broader than the normal
        /// request-time mapper only in one respect: after all strict title/date checks fail, an
        /// exact episode-number structure with at least 80% normalized title agreement may
        /// identify one unique season. Shifted/subset episode numbering is never accepted.
        /// </summary>
        public static SonarrSeasonFingerprintMatch FindSingleSeriesIdentityMatch(
            SeasonRequests sourceSeason,
            IEnumerable<Episode> sonarrEpisodes)
        {
            var episodes = sonarrEpisodes?
                .Where(x => x != null)
                .ToList() ?? new List<Episode>();

            var strictMatch = FindSingleSeasonMatch(sourceSeason, episodes);
            if (strictMatch != null)
            {
                return strictMatch;
            }

            // Parent identity repair may also use one strong exact-number structural/title match.
            // This is not enabled in the ordinary mapper because exact-season sends retain the
            // stricter outstanding-episode safeguards used for The Bad Guys-style title variants.
            var structuralTitleMatches = FindStrongStructuralTitleMatches(sourceSeason, episodes);
            if (structuralTitleMatches.Count != 1)
            {
                return null;
            }

            return new SonarrSeasonFingerprintMatch
            {
                SourceSeasonNumber = sourceSeason.SeasonNumber,
                SonarrSeasonNumber = structuralTitleMatches[0]
            };
        }

        private static List<int> FindStrongStructuralTitleMatches(
            SeasonRequests sourceSeason,
            IEnumerable<Episode> sonarrEpisodes)
        {
            var sourceEpisodes = sourceSeason?.Episodes?
                .Where(x => x != null)
                .GroupBy(x => x.EpisodeNumber)
                .Select(x => x.First())
                .OrderBy(x => x.EpisodeNumber)
                .ToList() ?? new List<EpisodeRequests>();
            var fingerprint = GetFingerprint(sourceSeason);

            if (sourceEpisodes.Count < MinimumEpisodeFingerprintSize ||
                fingerprint.Count < MinimumEpisodeFingerprintSize)
            {
                return new List<int>();
            }

            var sourceEpisodeNumbers = new HashSet<int>(sourceEpisodes.Select(x => x.EpisodeNumber));
            var strongMatches = new List<int>();

            foreach (var candidateSeason in (sonarrEpisodes ?? Enumerable.Empty<Episode>())
                .Where(x => x != null)
                .GroupBy(x => x.seasonNumber))
            {
                var candidateEpisodes = candidateSeason
                    .GroupBy(x => x.episodeNumber)
                    .Select(x => x.First())
                    .ToDictionary(x => x.episodeNumber);

                // Parent-series recovery must never infer an episode offset. Pahkitew-style
                // provider splits, where standalone E1 maps to consolidated E14, require an
                // explicit per-episode mapping and are intentionally left for manual handling.
                if (candidateEpisodes.Count != sourceEpisodes.Count ||
                    !sourceEpisodeNumbers.SetEquals(candidateEpisodes.Keys))
                {
                    continue;
                }

                var matchingTitleCount = fingerprint.Count(expected =>
                    candidateEpisodes.TryGetValue(expected.EpisodeNumber, out var actual) &&
                    TitlesMatch(actual.title, expected.NormalizedTitle));

                // Require at least 80% of the complete episode structure to agree by normalized
                // title, not merely 80% of whichever source episodes happen to have titles.
                if (matchingTitleCount >= MinimumEpisodeFingerprintSize &&
                    matchingTitleCount * 100 >= sourceEpisodes.Count * 80)
                {
                    strongMatches.Add(candidateSeason.Key);
                }
            }

            return strongMatches;
        }

        /// <summary>
        /// Returns true when Sonarr contains the source season number but the available episode
        /// evidence is not strong enough to use it safely. This protects anthology mappings from
        /// silently targeting the wrong season while allowing small provider title differences in
        /// an otherwise identical exact-number season.
        /// </summary>
        public static bool HasConflictingExactSeason(
            SeasonRequests sourceSeason,
            IEnumerable<Episode> sonarrEpisodes)
        {
            var fingerprint = GetFingerprint(sourceSeason);
            if (fingerprint.Count == 0)
            {
                return false;
            }

            var exactSeason = (sonarrEpisodes ?? Enumerable.Empty<Episode>())
                .Where(x => x != null && x.seasonNumber == sourceSeason.SeasonNumber)
                .GroupBy(x => x.episodeNumber)
                .ToDictionary(x => x.Key, x => x.First().title);

            if (exactSeason.Count == 0)
            {
                return false;
            }

            // Missing episode numbers are a structural disagreement, not a provider-title variant.
            // Keep treating those as unsafe.
            if (fingerprint.Any(expected => !exactSeason.ContainsKey(expected.EpisodeNumber)))
            {
                return true;
            }

            var mismatchedTitles = fingerprint
                .Where(expected => !TitlesMatch(exactSeason[expected.EpisodeNumber], expected.NormalizedTitle))
                .ToList();

            if (mismatchedTitles.Count == 0)
            {
                return false;
            }

            // TMDB and TVDB occasionally use different display names for a small number of
            // episodes in an otherwise identical season (for example "Fear the Ripper (2)" vs
            // "Fear the Ripper Pt. 2"). Do not reject the exact season solely because of those
            // unrelated title variants when the season structure strongly agrees and every
            // episode Ombi still needs is an exact number/title match.
            if (HasStrongExactSeasonEvidence(sourceSeason, fingerprint, exactSeason, mismatchedTitles.Count))
            {
                return false;
            }

            // A strong date fingerprint can safely corroborate the exact-number season even when
            // localized/provider-specific titles disagree. Do not accept it when another Sonarr
            // season satisfies the same date fingerprint; ambiguity must still fail closed.
            var airDateMatches = FindAirDateFingerprintMatches(sourceSeason, sonarrEpisodes);
            if (airDateMatches.Count == 1 && airDateMatches[0] == sourceSeason.SeasonNumber)
            {
                return false;
            }

            return true;
        }

        private static bool HasStrongExactSeasonEvidence(
            SeasonRequests sourceSeason,
            IReadOnlyCollection<EpisodeFingerprint> fingerprint,
            IReadOnlyDictionary<int, string> exactSeason,
            int mismatchedTitleCount)
        {
            // Require the providers to describe the same episode-number set. This keeps the
            // tolerance from masking anthology splits, missing episodes, or shifted numbering.
            if (exactSeason.Count != fingerprint.Count)
            {
                return false;
            }

            var unavailableEpisodes = sourceSeason?.Episodes?
                .Where(x => x != null && !x.Available)
                .GroupBy(x => x.EpisodeNumber)
                .Select(x => x.First())
                .ToList() ?? new List<EpisodeRequests>();

            // If Ombi has nothing outstanding, there is no actionable target to corroborate the
            // exact-season mapping. Keep the original strict behavior in that case.
            if (unavailableEpisodes.Count == 0)
            {
                return false;
            }

            // Every episode Ombi still needs must itself agree exactly by number and normalized
            // title. Provider-title tolerance is only allowed on episodes unrelated to the action.
            if (unavailableEpisodes.Any(expected =>
                string.IsNullOrWhiteSpace(expected.Title) ||
                !exactSeason.TryGetValue(expected.EpisodeNumber, out var actualTitle) ||
                !TitlesMatch(actualTitle, NormalizeTitle(expected.Title))))
            {
                return false;
            }

            var matchingTitleCount = fingerprint.Count - mismatchedTitleCount;
            if (matchingTitleCount < MinimumEpisodeFingerprintSize)
            {
                return false;
            }

            // Require at least 80% of the full season fingerprint to agree exactly. This is high
            // enough to preserve the anthology safety check while tolerating a small number of
            // provider naming differences in a structurally identical season.
            return matchingTitleCount * 100 >= fingerprint.Count * 80;
        }

        private static List<int> FindAirDateFingerprintMatches(
            SeasonRequests sourceSeason,
            IEnumerable<Episode> sonarrEpisodes)
        {
            var sourceEpisodes = sourceSeason?.Episodes?
                .Where(x => x != null)
                .GroupBy(x => x.EpisodeNumber)
                .Select(x => x.First())
                .OrderBy(x => x.EpisodeNumber)
                .ToList() ?? new List<EpisodeRequests>();

            if (sourceEpisodes.Count < MinimumEpisodeFingerprintSize)
            {
                return new List<int>();
            }

            var sourceEpisodeNumbers = new HashSet<int>(sourceEpisodes.Select(x => x.EpisodeNumber));
            var sourceDatedEpisodeCount = sourceEpisodes.Count(x => HasUsableAirDate(x.AirDate));
            if (sourceDatedEpisodeCount < MinimumEpisodeFingerprintSize)
            {
                return new List<int>();
            }

            var matches = new List<int>();
            var candidateSeasons = (sonarrEpisodes ?? Enumerable.Empty<Episode>())
                .Where(x => x != null)
                .GroupBy(x => x.seasonNumber);

            foreach (var candidateSeason in candidateSeasons)
            {
                var candidateEpisodes = candidateSeason
                    .GroupBy(x => x.episodeNumber)
                    .Select(x => x.First())
                    .ToDictionary(x => x.episodeNumber);

                // Air dates are only corroborating evidence after the providers agree on the exact
                // episode-number structure. Never use dates to paper over splits, missing episodes,
                // extra episodes, or shifted numbering.
                if (candidateEpisodes.Count != sourceEpisodes.Count ||
                    !sourceEpisodeNumbers.SetEquals(candidateEpisodes.Keys))
                {
                    continue;
                }

                var comparableDateCount = 0;
                var matchingDateCount = 0;

                foreach (var sourceEpisode in sourceEpisodes)
                {
                    if (!HasUsableAirDate(sourceEpisode.AirDate) ||
                        !TryGetSonarrAirDate(candidateEpisodes[sourceEpisode.EpisodeNumber], out var sonarrAirDate))
                    {
                        continue;
                    }

                    comparableDateCount++;
                    var difference = Math.Abs((sourceEpisode.AirDate.Date - sonarrAirDate.Date).TotalDays);
                    if (difference <= AirDateToleranceDays)
                    {
                        matchingDateCount++;
                    }
                }

                if (comparableDateCount < MinimumEpisodeFingerprintSize)
                {
                    continue;
                }

                // Sparse metadata must not look convincing merely because the few available dates
                // happen to agree. Require comparable dates for at least 80% of the full season.
                if (comparableDateCount * 100 < sourceEpisodes.Count * MinimumComparableAirDatePercentage)
                {
                    continue;
                }

                if (matchingDateCount * 100 < comparableDateCount * MinimumMatchingAirDatePercentage)
                {
                    continue;
                }

                matches.Add(candidateSeason.Key);
            }

            return matches;
        }

        private static bool HasUsableAirDate(DateTime airDate)
        {
            return airDate != DateTime.MinValue;
        }

        private static bool TryGetSonarrAirDate(Episode episode, out DateTime airDate)
        {
            if (!string.IsNullOrWhiteSpace(episode?.airDate) &&
                DateTime.TryParse(
                    episode.airDate,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces,
                    out var parsedAirDate))
            {
                airDate = parsedAirDate.Date;
                return true;
            }

            airDate = default;
            return false;
        }

        private static List<EpisodeFingerprint> GetFingerprint(SeasonRequests sourceSeason)
        {
            return sourceSeason?.Episodes?
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Title))
                .GroupBy(x => x.EpisodeNumber)
                .Select(x => x.First())
                .Select(x => new EpisodeFingerprint
                {
                    EpisodeNumber = x.EpisodeNumber,
                    NormalizedTitle = NormalizeTitle(x.Title)
                })
                .Where(x => !string.IsNullOrEmpty(x.NormalizedTitle))
                .OrderBy(x => x.EpisodeNumber)
                .ToList() ?? new List<EpisodeFingerprint>();
        }

        private static bool TitlesMatch(string actualTitle, string normalizedExpectedTitle)
        {
            return string.Equals(
                NormalizeTitle(actualTitle),
                normalizedExpectedTitle,
                StringComparison.Ordinal);
        }

        private static string NormalizeTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return string.Empty;
            }

            return new string(title
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());
        }

        private sealed class EpisodeFingerprint
        {
            public int EpisodeNumber { get; set; }
            public string NormalizedTitle { get; set; }
        }
    }

    public sealed class SonarrSeasonFingerprintMatch
    {
        public int SourceSeasonNumber { get; set; }
        public int SonarrSeasonNumber { get; set; }
    }
}
