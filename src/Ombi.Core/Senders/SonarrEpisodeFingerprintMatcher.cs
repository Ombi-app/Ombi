using System;
using System.Collections.Generic;
using System.Linq;
using Ombi.Api.External.ExternalApis.Sonarr.Models;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Senders
{
    /// <summary>
    /// Resolves a TMDB-style requested season to the corresponding Sonarr season when providers
    /// disagree about whether an anthology season is a standalone show. Remaps are intentionally
    /// conservative: at least three requested episode titles must identify exactly one Sonarr season.
    /// Exact-number seasons may tolerate a small number of provider title variants only when the
    /// episode structure strongly agrees and every episode Ombi still needs matches exactly.
    /// </summary>
    public static class SonarrEpisodeFingerprintMatcher
    {
        private const int MinimumEpisodeFingerprintSize = 3;

        public static SonarrSeasonFingerprintMatch FindSingleSeasonMatch(
            SeasonRequests sourceSeason,
            IEnumerable<Episode> sonarrEpisodes)
        {
            var fingerprint = GetFingerprint(sourceSeason);
            if (fingerprint.Count < MinimumEpisodeFingerprintSize)
            {
                return null;
            }

            var episodes = sonarrEpisodes?
                .Where(x => x != null)
                .ToList() ?? new List<Episode>();

            if (episodes.Count == 0)
            {
                return null;
            }

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

            var matches = new List<int>();
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
                    matches.Add(candidateSeason);
                }
            }

            // Never guess when more than one Sonarr season satisfies the fingerprint.
            if (matches.Count != 1)
            {
                return null;
            }

            return new SonarrSeasonFingerprintMatch
            {
                SourceSeasonNumber = sourceSeason.SeasonNumber,
                SonarrSeasonNumber = matches[0]
            };
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
