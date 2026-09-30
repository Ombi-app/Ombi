using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.External.ExternalApis.SickRage;
using Ombi.Api.External.ExternalApis.SickRage.Models;
using Ombi.Api.External.ExternalApis.Sonarr;
using Ombi.Api.External.ExternalApis.Sonarr.Models;
using Ombi.Api.External.ExternalApis.TheMovieDb;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Senders
{
    public class TvSender : ITvSender
    {
        public TvSender(ISonarrV3Api sonarrV3Api, ILogger<TvSender> log, ISettingsService<SonarrSettings> sonarrSettings,
            ISettingsService<SickRageSettings> srSettings, IMovieDbApi movieDbApi, ITvRequestRepository tvRequestRepository,
            ISickRageApi srApi, IRepository<UserQualityProfiles> userProfiles, IRepository<RequestQueue> requestQueue, INotificationHelper notify)
        {
            SonarrApi = sonarrV3Api;
            Logger = log;
            SonarrSettings = sonarrSettings;
            SickRageSettings = srSettings;
            MovieDbApi = movieDbApi;
            TvRequestRepository = tvRequestRepository;
            SickRageApi = srApi;
            UserQualityProfiles = userProfiles;
            _requestQueueRepository = requestQueue;
            _notificationHelper = notify;
        }

        private ISonarrV3Api SonarrApi { get; }
        private ISickRageApi SickRageApi { get; }
        private ILogger<TvSender> Logger { get; }
        private ISettingsService<SonarrSettings> SonarrSettings { get; }
        private ISettingsService<SickRageSettings> SickRageSettings { get; }
        private IMovieDbApi MovieDbApi { get; }
        private ITvRequestRepository TvRequestRepository { get; }
        private IRepository<UserQualityProfiles> UserQualityProfiles { get; }
        private readonly IRepository<RequestQueue> _requestQueueRepository;
        private readonly INotificationHelper _notificationHelper;

        public async Task<SenderResult> Send(ChildRequests model)
        {
            SenderResult senderResult = null;
            try
            {
                var sonarr = await SonarrSettings.GetSettingsAsync();
                if (sonarr.Enabled)
                {
                    var result = await SendToSonarr(model, sonarr);
                    if (result != null)
                    {
                        return new SenderResult
                        {
                            Sent = true,
                            Success = true
                        };
                    }
                }

                var sr = await SickRageSettings.GetSettingsAsync();
                if (sr.Enabled)
                {
                    var result = await SendToSickRage(model, sr);
                    if (result)
                    {
                        return new SenderResult
                        {
                            Sent = true,
                            Success = true
                        };
                    }
                    senderResult = new SenderResult
                    {
                        Message = "Could not send to SickRage!"
                    };
                }
                else
                {
                    return new SenderResult
                    {
                        Success = true
                    };
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Exception thrown when sending a series to DVR app, added to the request queue");
                await AddToRequestFailureQueue(model, e.Message);
            }

            if (senderResult != null && !senderResult.Success)
            {
                Logger.LogWarning("TV send to DVR app failed: {Message}, added to the request queue", senderResult.Message);
                await AddToRequestFailureQueue(model, senderResult.Message);
                return senderResult;
            }

            return new SenderResult
            {
                Success = false
            };
        }

        /// <summary>
        /// Send the request to Sonarr to process
        /// </summary>
        /// <param name="s"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<NewSeries> SendToSonarr(ChildRequests model, SonarrSettings s)
        {
            if (string.IsNullOrEmpty(s.ApiKey))
            {
                return null;
            }

            await EnsureTvDbId(model, s);

            var options = new SonarrSendOptions();

            int qualityToUse;
            var languageProfileId = s.LanguageProfile;
            string rootFolderPath;
            string seriesType;
            int? tagToUse = null;

            Logger.LogInformation("Starting SendToSonarr for series {Title} (TvDbId: {TvDbId})", model.ParentRequest.Title, model.ParentRequest.TvDbId);
            Logger.LogInformation("Series type: {SeriesType}", model.SeriesType);

            var profiles = await UserQualityProfiles.GetAll().FirstOrDefaultAsync(x => x.UserId == model.RequestedUserId);
            if (profiles != null)
            {
                Logger.LogInformation("Found user quality profile for user {UserId}", model.RequestedUserId);
            }

            if (model.SeriesType == SeriesType.Anime)
            {
                // Get the root path from the rootfolder selected.
                // For some reason, if we haven't got one use the first root folder in Sonarr
                if (!int.TryParse(s.RootPathAnime, out int animePath))
                {
                    Logger.LogWarning("Failed to parse RootPathAnime: {RootPathAnime}, falling back to main root path", s.RootPathAnime);
                    animePath = int.Parse(s.RootPath); // Set it to the main root folder if we have no anime folder.
                }
                Logger.LogInformation("Using anime path ID: {AnimePath}", animePath);
                rootFolderPath = await GetSonarrRootPath(animePath, s);
                languageProfileId = s.LanguageProfileAnime > 0 ? s.LanguageProfileAnime : s.LanguageProfile;

                if (!int.TryParse(s.QualityProfileAnime, out qualityToUse))
                {
                    qualityToUse = int.Parse(s.QualityProfile);
                }
                if (profiles != null)
                {
                    if (profiles.SonarrRootPathAnime > 0)
                    {
                        Logger.LogInformation("Checking user's anime root path override: {RootPath}", profiles.SonarrRootPathAnime);
                        var userAnimeRootPath = await GetSonarrRootPath(profiles.SonarrRootPathAnime, s);
                        // Only use the user's root path if it's valid (exists in Sonarr)
                        if (!string.IsNullOrEmpty(userAnimeRootPath))
                        {
                            Logger.LogInformation("Using user's anime root path override: {RootPath}", profiles.SonarrRootPathAnime);
                            rootFolderPath = userAnimeRootPath;
                        }
                        else
                        {
                            Logger.LogWarning("User's anime root path ID {RootPath} no longer exists in Sonarr, falling back to global default", profiles.SonarrRootPathAnime);
                        }
                    }
                    if (profiles.SonarrQualityProfileAnime > 0)
                    {
                        qualityToUse = profiles.SonarrQualityProfileAnime;
                    }
                }
                seriesType = "anime";
                tagToUse = s.AnimeTag;
            }
            else
            {
                int.TryParse(s.QualityProfile, out qualityToUse);
                // Get the root path from the rootfolder selected.
                // For some reason, if we haven't got one use the first root folder in Sonarr
                Logger.LogInformation("Using standard path ID: {RootPath}", s.RootPath);
                rootFolderPath = await GetSonarrRootPath(int.Parse(s.RootPath), s);
                if (profiles != null)
                {
                    if (profiles.SonarrRootPath > 0)
                    {
                        Logger.LogInformation("Checking user's standard root path override: {RootPath}", profiles.SonarrRootPath);
                        var userRootPath = await GetSonarrRootPath(profiles.SonarrRootPath, s);
                        // Only use the user's root path if it's valid (exists in Sonarr)
                        if (!string.IsNullOrEmpty(userRootPath))
                        {
                            Logger.LogInformation("Using user's standard root path override: {RootPath}", profiles.SonarrRootPath);
                            rootFolderPath = userRootPath;
                        }
                        else
                        {
                            Logger.LogWarning("User's standard root path ID {RootPath} no longer exists in Sonarr, falling back to global default", profiles.SonarrRootPath);
                        }
                    }
                    if (profiles.SonarrQualityProfile > 0)
                    {
                        qualityToUse = profiles.SonarrQualityProfile;
                    }
                }
                seriesType = "standard";
                tagToUse = s.Tag;
            }

            // A child-level value records what was selected for this specific request so
            // pending approvals cannot lose their profile choice. ParentRequest remains the
            // series-wide fallback for older requests and subsequent requests with no choice.
            var currentRequestHasProfileChoice = model.QualityOverride.HasValue;
            var currentRequestQualityOverride = model.QualityOverride.GetValueOrDefault();
            var profileOverrideRequested = currentRequestHasProfileChoice && currentRequestQualityOverride > 0;

            if (currentRequestHasProfileChoice)
            {
                if (currentRequestQualityOverride > 0)
                {
                    qualityToUse = currentRequestQualityOverride;
                }
                // A zero child value explicitly means no request-level override; do not fall
                // back to a previous parent override for this request.
            }
            else if (model.ParentRequest.QualityOverride.HasValue && model.ParentRequest.QualityOverride.Value > 0)
            {
                qualityToUse = model.ParentRequest.QualityOverride.Value;
            }

            if (model.ParentRequest.RootFolder.HasValue && model.ParentRequest.RootFolder.Value > 0)
            {
                Logger.LogInformation("Using request root folder override: {RootFolder}", model.ParentRequest.RootFolder.Value);
                rootFolderPath = await GetSonarrRootPath(model.ParentRequest.RootFolder.Value, s);
            }

            if (model.ParentRequest.LanguageProfile.HasValue && model.ParentRequest.LanguageProfile.Value > 0)
            {
                languageProfileId = model.ParentRequest.LanguageProfile.Value;
            }

            Logger.LogInformation("Final root folder path: {RootFolderPath}", rootFolderPath);

            try
            {
                if (tagToUse.HasValue)
                {
                    options.Tags.Add(tagToUse.Value);
                }
                if (s.SendUserTags)
                {
                    var userTag = await GetOrCreateTag(model, s);
                    if (userTag != null)
                    {
                        options.Tags.Add(userTag.id);
                    }
                }

                // Does the series actually exist?
                var allSeries = await SonarrApi.GetSeries(s.ApiKey, s.FullUri);
                var existingSeries = allSeries.FirstOrDefault(x => x.tvdbId == model.ParentRequest.TvDbId);

                if (existingSeries == null)
                {
                    // Time to add a new one
                    var newSeries = new NewSeries
                    {
                        title = model.ParentRequest.Title,
                        imdbId = model.ParentRequest.ImdbId,
                        tvdbId = model.ParentRequest.TvDbId,
                        cleanTitle = model.ParentRequest.Title,
                        monitored = true,
                        seasonFolder = s.SeasonFolders,
                        rootFolderPath = rootFolderPath,
                        qualityProfileId = qualityToUse,
                        titleSlug = model.ParentRequest.Title,
                        seriesType = seriesType,
                        addOptions = new AddOptions
                        {
                            ignoreEpisodesWithFiles = false, // There shouldn't be any episodes with files, this is a new season
                            ignoreEpisodesWithoutFiles = false, // We want all missing
                            searchForMissingEpisodes = false // we want dont want to search yet. We want to make sure everything is unmonitored/monitored correctly.
                        },
                        languageProfileId = languageProfileId,
                        tags = options.Tags
                    };


                    // Montitor the correct seasons,
                    // If we have that season in the model then it's monitored!
                    var seasonsToAdd = GetSeasonsToCreate(model);
                    newSeries.seasons = seasonsToAdd;
                    var result = await SonarrApi.AddSeries(newSeries, s.ApiKey, s.FullUri);
                    if (result?.ErrorMessages?.Any() ?? false)
                    {
                        throw new Exception(string.Join(',', result.ErrorMessages));
                    }
                    if (result == null || result.id <= 0)
                    {
                        throw new InvalidOperationException(
                            $"Sonarr did not return a valid series identifier after adding '{model.ParentRequest.Title}'.");
                    }

                    // Sonarr creates a new series before Ombi can validate/apply its final episode
                    // monitoring state. If any post-add configuration step fails, remove only the
                    // series record created by this operation. Never delete files or add an import
                    // exclusion during this compensating rollback.
                    try
                    {
                        existingSeries = await SonarrApi.GetSeriesById(result.id, s.ApiKey, s.FullUri);
                        if (existingSeries == null)
                        {
                            throw new InvalidOperationException(
                                $"Sonarr returned no series metadata after adding '{model.ParentRequest.Title}' (series id {result.id}).");
                        }

                        await SendToSonarr(model, existingSeries, s, options);
                    }
                    catch (Exception configurationException)
                    {
                        await TryRollbackNewSonarrSeries(result.id, model.ParentRequest.Title, s, configurationException);
                        throw;
                    }
                }
                else
                {
                    var seriesNeedsUpdate = false;
                    if (profileOverrideRequested && existingSeries.qualityProfileId != qualityToUse)
                    {
                        // Sonarr quality profiles are series-wide. Selecting a new profile for
                        // any season therefore reprofiles the whole existing series.
                        existingSeries.qualityProfileId = qualityToUse;
                        seriesNeedsUpdate = true;
                    }

                    if (existingSeries is { monitored: false })
                    {
                        existingSeries.monitored = true;
                        seriesNeedsUpdate = true;
                    }

                    if (seriesNeedsUpdate)
                    {
                        existingSeries = await SonarrApi.UpdateSeries(existingSeries, s.ApiKey, s.FullUri);
                    }

                    // SendToSonarr performs the request-scoped season/episode search after
                    // applying any series-wide profile update. Do not follow it with SeriesSearch:
                    // choosing a profile for one request must not implicitly search every monitored
                    // season in an existing Sonarr series.
                    await SendToSonarr(model, existingSeries, s, options);
                }

                return new NewSeries
                {
                    id = existingSeries.id,
                    seasons = existingSeries.seasons.ToList(),
                    cleanTitle = existingSeries.cleanTitle,
                    title = existingSeries.title,
                    tvdbId = existingSeries.tvdbId
                };
            }
            catch (Exception e)
            {
                Logger.LogError(LoggingEvents.SonarrSender, e, "Exception thrown when attempting to send series over to Sonarr");
                throw;
            }
        }

        private async Task EnsureTvDbId(ChildRequests model, SonarrSettings settings)
        {
            if (model?.ParentRequest == null || model.ParentRequest.TvDbId > 0)
            {
                return;
            }

            var parent = model.ParentRequest;
            Ombi.Api.External.ExternalApis.TheMovieDb.Models.TvExternals externalIds = null;

            if (parent.ExternalProviderId > 0)
            {
                Logger.LogWarning(
                    "TV request {RequestId} for {Title} is missing a TVDB ID; refreshing TMDB external IDs for TMDB {TmdbId}",
                    model.Id, parent.Title, parent.ExternalProviderId);

                // Let network/API exceptions propagate normally. Those are transient failures and
                // should remain eligible for the regular failed-request retry mechanism.
                externalIds = await MovieDbApi.GetTvExternals(parent.ExternalProviderId);
                if (string.IsNullOrEmpty(parent.ImdbId) && !string.IsNullOrEmpty(externalIds?.imdb_id))
                {
                    parent.ImdbId = externalIds.imdb_id;
                }

                if (externalIds?.tvdb_id > 0)
                {
                    parent.TvDbId = externalIds.tvdb_id;
                    await TvRequestRepository.Save();

                    Logger.LogInformation(
                        "Repaired TV request {RequestId} for {Title}: TMDB {TmdbId} -> TVDB {TvdbId}",
                        model.Id, parent.Title, parent.ExternalProviderId, parent.TvDbId);
                    return;
                }
            }

            // Some anthology seasons are exposed by TMDB as standalone series while Sonarr/TVDB
            // keeps them under the anthology parent. If TMDB cannot provide a TVDB mapping, use
            // Sonarr's own series metadata. Prefer provider IDs and only then accept one unique
            // normalized title/alternate-title match.
            var allSeries = (await SonarrApi.GetSeries(settings.ApiKey, settings.FullUri))?.ToList()
                ?? new List<SonarrSeries>();

            SonarrSeries sonarrMatch = null;
            if (parent.ExternalProviderId > 0)
            {
                sonarrMatch = GetUniqueSeriesMatch(allSeries,
                    x => x.tmdbId == parent.ExternalProviderId);
            }

            if (sonarrMatch == null && !string.IsNullOrWhiteSpace(parent.ImdbId))
            {
                sonarrMatch = GetUniqueSeriesMatch(allSeries,
                    x => string.Equals(x.imdbId, parent.ImdbId, StringComparison.OrdinalIgnoreCase));
            }

            if (sonarrMatch == null)
            {
                var normalizedRequestTitle = NormalizeSeriesTitle(parent.Title);
                if (!string.IsNullOrEmpty(normalizedRequestTitle))
                {
                    var titleMatches = allSeries
                        .Where(x =>
                            NormalizeSeriesTitle(x?.title) == normalizedRequestTitle ||
                            (x?.alternateTitles?.Any(a =>
                                NormalizeSeriesTitle(a?.title) == normalizedRequestTitle) ?? false))
                        .Take(2)
                        .ToList();

                    if (titleMatches.Count == 1)
                    {
                        sonarrMatch = titleMatches[0];
                    }
                    else if (titleMatches.Count > 1)
                    {
                        Logger.LogWarning(
                            "Could not repair TVDB ID for TV request {RequestId} ({Title}) because multiple Sonarr series matched the normalized title/alternate title",
                            model.Id, parent.Title);
                    }
                }
            }

            if (sonarrMatch?.tvdbId > 0)
            {
                parent.TvDbId = sonarrMatch.tvdbId;
                if (string.IsNullOrEmpty(parent.ImdbId) && !string.IsNullOrEmpty(sonarrMatch.imdbId))
                {
                    parent.ImdbId = sonarrMatch.imdbId;
                }

                await TvRequestRepository.Save();

                Logger.LogInformation(
                    "Repaired TV request {RequestId} for {Title} from existing Sonarr series {SonarrTitle}: TVDB {TvdbId}",
                    model.Id, parent.Title, sonarrMatch.title, parent.TvDbId);
                return;
            }

            if (parent.ExternalProviderId <= 0)
            {
                throw new MissingTvDbIdException(
                    $"{MissingTvDbAfterRefreshPrefix}: '{parent.Title}' (child request {model.Id}) has no TMDB ID and no unique existing Sonarr mapping.");
            }

            throw new MissingTvDbIdException(
                $"{MissingTvDbAfterRefreshPrefix}: '{parent.Title}' (child request {model.Id}, TMDB {parent.ExternalProviderId}) still has no TVDB mapping and no unique existing Sonarr match.");
        }

        private static SonarrSeries GetUniqueSeriesMatch(IEnumerable<SonarrSeries> series, Func<SonarrSeries, bool> predicate)
        {
            var matches = series.Where(x => x != null && predicate(x)).Take(2).ToList();
            return matches.Count == 1 ? matches[0] : null;
        }

        private static string NormalizeSeriesTitle(string title)
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

        public const string MissingTvDbAfterRefreshPrefix = "TVDBID is missing after TMDB external-id refresh";


        private sealed class UnsafeSeasonMappingException : InvalidOperationException
        {
            public UnsafeSeasonMappingException(string message) : base(message)
            {
            }
        }

        private sealed class MissingTvDbIdException : Exception
        {
            public MissingTvDbIdException(string message) : base(message)
            {
            }
        }

        private async Task<Tag> GetOrCreateTag(ChildRequests model, SonarrSettings s)
        {
            // Sanitize username to comply with Sonarr tag requirements (a-z, 0-9, and - only)
            var tagName = StringHelper.SanitizeTagLabel(model.RequestedUser.UserName);

            if (string.IsNullOrEmpty(tagName))
            {
                Logger.LogWarning("Cannot create tag - sanitized username is empty for user {Username}", model.RequestedUser.UserName);
                return null;
            }

            // Does tag exist?
            var allTags = await SonarrApi.GetTags(s.ApiKey, s.FullUri);
            var existingTag = allTags.FirstOrDefault(x => x.label.Equals(tagName, StringComparison.InvariantCultureIgnoreCase));
            existingTag ??= await SonarrApi.CreateTag(s.ApiKey, s.FullUri, tagName);

            return existingTag;
        }

        private async Task<Tag> GetTag(int tagId, SonarrSettings s)
        {
            var tag = await SonarrApi.GetTag(tagId, s.ApiKey, s.FullUri);
            if (tag == null)
            {
                Logger.LogError($"Tag ID {tagId} does not exist in sonarr. Please update the settings");
                return null;
            }
            return tag;
        }

        private async Task SendToSonarr(ChildRequests model, SonarrSeries result, SonarrSettings s, SonarrSendOptions options)
        {
            // Does the show have the correct tags we are expecting
            if (options.Tags.Any())
            {
                result.tags ??= options.Tags;
                var tagsToAdd = options.Tags.Except(result.tags);

                if (tagsToAdd.Any())
                {
                    result.tags.AddRange(tagsToAdd);
                }
                result = await SonarrApi.UpdateSeries(result, s.ApiKey, s.FullUri);
            }

            if (model.SeriesType == SeriesType.Anime)
            {
                result.seriesType = "anime";
                result = await SonarrApi.UpdateSeries(result, s.ApiKey, s.FullUri);
            }

            // Sonarr can briefly return a series before its episode metadata is ready. Bound the
            // wait so a bad Sonarr response cannot pin a request worker forever.
            const int maxEpisodeMetadataAttempts = 20;
            var episodeMetadataAttempt = 0;
            var sonarrEpList = (await SonarrApi.GetEpisodes(result.id, s.ApiKey, s.FullUri))?.ToList()
                ?? new List<Episode>();
            while (!sonarrEpList.Any() && episodeMetadataAttempt < maxEpisodeMetadataAttempts)
            {
                episodeMetadataAttempt++;
                await Task.Delay(500);
                sonarrEpList = (await SonarrApi.GetEpisodes(result.id, s.ApiKey, s.FullUri))?.ToList()
                    ?? new List<Episode>();
            }

            if (!sonarrEpList.Any())
            {
                throw new InvalidOperationException(
                    $"Sonarr returned no episode metadata for '{model.ParentRequest.Title}' after {maxEpisodeMetadataAttempts} retries.");
            }

            // Provider splits can expose an anthology season as a standalone TMDB show. Resolve
            // the requested season to Sonarr by episode title/number before changing monitoring.
            // Never mutate Ombi's stored season number; the mapping only applies to this send.
            var seasonNumberMap = new Dictionary<int, int>();
            foreach (var season in model.SeasonRequests)
            {
                var fingerprintMatch = SonarrEpisodeFingerprintMatcher.FindSingleSeasonMatch(season, sonarrEpList);
                if (fingerprintMatch != null)
                {
                    seasonNumberMap[season.SeasonNumber] = fingerprintMatch.SonarrSeasonNumber;
                    if (fingerprintMatch.SonarrSeasonNumber != season.SeasonNumber)
                    {
                        Logger.LogInformation(
                            "Mapped requested season {SourceSeason} for {Title} to Sonarr season {SonarrSeason} using episode fingerprints",
                            season.SeasonNumber, model.ParentRequest.Title, fingerprintMatch.SonarrSeasonNumber);
                    }
                    continue;
                }

                if (SonarrEpisodeFingerprintMatcher.HasConflictingExactSeason(season, sonarrEpList))
                {
                    throw new UnsafeSeasonMappingException(
                        $"Unable to safely map requested season {season.SeasonNumber} for '{model.ParentRequest.Title}' to the existing Sonarr series. " +
                        "The same season number exists in Sonarr but its episode titles do not match, and no unique episode fingerprint match was found.");
                }

                seasonNumberMap[season.SeasonNumber] = season.SeasonNumber;
            }

            // Ensure every mapped season exists. This is mainly needed just after adding a new
            // series, where the series record may appear before Sonarr finishes creating seasons.
            Season existingSeason = null;
            foreach (var season in model.SeasonRequests)
            {
                var targetSeasonNumber = seasonNumberMap[season.SeasonNumber];
                var attempt = 0;
                existingSeason = result.seasons?.FirstOrDefault(x => x.seasonNumber == targetSeasonNumber);
                while (existingSeason == null && attempt < 5)
                {
                    attempt++;
                    Logger.LogInformation(
                        "There was no Sonarr season {SonarrSeason} for requested season {SourceSeason} in title {Title}. Will try again as the metadata may not be ready",
                        targetSeasonNumber, season.SeasonNumber, model.ParentRequest.Title);
                    result = await SonarrApi.GetSeriesById(result.id, s.ApiKey, s.FullUri);
                    existingSeason = result.seasons?.FirstOrDefault(x => x.seasonNumber == targetSeasonNumber);
                    await Task.Delay(500);
                }

                if (existingSeason == null)
                {
                    Logger.LogWarning(
                        "Unable to locate Sonarr season {SonarrSeason} for requested season {SourceSeason} in title {Title} after {Attempts} attempts. Skipping monitoring updates for this season.",
                        targetSeasonNumber, season.SeasonNumber, model.ParentRequest.Title, attempt);
                }
            }

            var episodesToUpdate = new List<Episode>();
            foreach (var season in model.SeasonRequests)
            {
                var targetSeasonNumber = seasonNumberMap[season.SeasonNumber];
                foreach (var ep in season.Episodes)
                {
                    var sonarrEp = sonarrEpList.FirstOrDefault(x =>
                        x.episodeNumber == ep.EpisodeNumber && x.seasonNumber == targetSeasonNumber);
                    if (sonarrEp != null && !sonarrEp.monitored)
                    {
                        sonarrEp.monitored = true;
                        episodesToUpdate.Add(sonarrEp);
                    }
                }

                existingSeason = result.seasons?.FirstOrDefault(x => x.seasonNumber == targetSeasonNumber);

                if (existingSeason == null)
                {
                    Logger.LogWarning(
                        "Sonarr season {SonarrSeason} for requested season {SourceSeason} is still missing for title {Title}; skipping monitoring changes for this season.",
                        targetSeasonNumber, season.SeasonNumber, model.ParentRequest.Title);
                    continue;
                }

                // Make sure this season is set to monitored. Sonarr monitors every episode when a
                // season is enabled, so reset the season's episodes and then enable only the ones
                // represented by the Ombi request.
                if (!existingSeason.monitored)
                {
                    existingSeason.monitored = true;
                    var sea = result.seasons.FirstOrDefault(x => x.seasonNumber == existingSeason.seasonNumber);
                    if (sea != null)
                    {
                        sea.monitored = true;
                    }

                    result = await SonarrApi.UpdateSeries(result, s.ApiKey, s.FullUri);
                    var epToUnmonitored = new List<Episode>();
                    var newEpList = sonarrEpList.ConvertAll(ep => new Episode(ep));
                    foreach (var ep in newEpList.Where(x => x.seasonNumber == existingSeason.seasonNumber))
                    {
                        ep.monitored = false;
                        epToUnmonitored.Add(ep);
                    }

                    if (epToUnmonitored.Any())
                    {
                        await SonarrApi.MonitorEpisode(epToUnmonitored.Select(x => x.id).ToArray(), false, s.ApiKey, s.FullUri);
                    }
                }
            }

            if (episodesToUpdate.Any())
            {
                await SonarrApi.MonitorEpisode(episodesToUpdate.Select(x => x.id).Distinct().ToArray(), true, s.ApiKey, s.FullUri);
            }

            if (!s.AddOnly)
            {
                await SearchForRequest(model, sonarrEpList, result, s, episodesToUpdate, seasonNumberMap);
            }
        }


        private async Task TryRollbackNewSonarrSeries(
            int seriesId,
            string title,
            SonarrSettings settings,
            Exception configurationException)
        {
            try
            {
                var removed = await SonarrApi.DeleteSeries(
                    seriesId,
                    settings.ApiKey,
                    settings.FullUri,
                    deleteFiles: false,
                    addImportListExclusion: false);

                if (removed)
                {
                    Logger.LogWarning(
                        "Rolled back newly-added Sonarr series {SeriesId} ({Title}) after post-add configuration failed: {ConfigurationError}",
                        seriesId, title, configurationException.Message);
                }
                else
                {
                    Logger.LogError(
                        "Sonarr did not confirm rollback of newly-added series {SeriesId} ({Title}) after post-add configuration failed: {ConfigurationError}",
                        seriesId, title, configurationException.Message);
                }
            }
            catch (Exception rollbackException)
            {
                // Preserve the original configuration failure. A rollback failure is important to
                // surface, but must not replace the exception that explains why the send failed.
                Logger.LogError(
                    rollbackException,
                    "Failed to roll back newly-added Sonarr series {SeriesId} ({Title}) after post-add configuration failed: {ConfigurationError}",
                    seriesId, title, configurationException.Message);
            }
        }

        private static List<Season> GetSeasonsToCreate(ChildRequests model)
        {
            // Let's get a list of seasons just incase we need to change it
            var seasonsToUpdate = new List<Season>();
            for (var i = 0; i < model.ParentRequest.TotalSeasons + 1; i++)
            {
                var sea = new Season
                {
                    seasonNumber = i,
                    monitored = false
                };
                seasonsToUpdate.Add(sea);
            }

            return seasonsToUpdate;
        }

        private async Task<bool> SendToSickRage(ChildRequests model, SickRageSettings settings, string qualityId = null)
        {
            var tvdbid = model.ParentRequest.TvDbId;
            if (qualityId.HasValue())
            {
                var id = qualityId;
                if (settings.Qualities.All(x => x.Value != id))
                {
                    qualityId = settings.QualityProfile;
                }
            }
            else
            {
                qualityId = settings.QualityProfile;
            }
            // Check if the show exists
            var existingShow = await SickRageApi.GetShow(tvdbid, settings.ApiKey, settings.FullUri);

            if (existingShow.message.Equals("Show not found", StringComparison.CurrentCultureIgnoreCase))
            {
                var addResult = await SickRageApi.AddSeries(model.ParentRequest.TvDbId, qualityId, SickRageStatus.Ignored,
                    settings.ApiKey, settings.FullUri);

                Logger.LogDebug("Added the show (tvdbid) {0}. The result is '{2}' : '{3}'", tvdbid, addResult.result, addResult.message);
                if (addResult.result.Equals("failure") || addResult.result.Equals("fatal"))
                {
                    // Do something
                    return false;
                }
            }

            foreach (var seasonRequests in model.SeasonRequests)
            {
                var srEpisodes = await SickRageApi.GetEpisodesForSeason(tvdbid, seasonRequests.SeasonNumber, settings.ApiKey, settings.FullUri);
                int retryTimes = 10;
                var currentRetry = 0;
                while (srEpisodes.message.Equals("Show not found", StringComparison.CurrentCultureIgnoreCase) || srEpisodes.message.Equals("Season not found", StringComparison.CurrentCultureIgnoreCase) && srEpisodes.data.Count <= 0)
                {
                    if (currentRetry > retryTimes)
                    {
                        Logger.LogWarning("Couldnt find the SR Season or Show, message: {0}", srEpisodes.message);
                        break;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    currentRetry++;
                    srEpisodes = await SickRageApi.GetEpisodesForSeason(tvdbid, seasonRequests.SeasonNumber, settings.ApiKey, settings.FullUri);
                }

                var totalSrEpisodes = srEpisodes.data.Count;

                if (totalSrEpisodes == seasonRequests.Episodes.Count)
                {
                    // This is a request for the whole season
                    var wholeSeasonResult = await SickRageApi.SetEpisodeStatus(settings.ApiKey, settings.FullUri, tvdbid, SickRageStatus.Wanted,
                        seasonRequests.SeasonNumber);

                    Logger.LogDebug("Set the status to Wanted for season {0}. The result is '{1}' : '{2}'", seasonRequests.SeasonNumber, wholeSeasonResult.result, wholeSeasonResult.message);
                    continue;
                }

                foreach (var srEp in srEpisodes.data)
                {
                    var epNumber = srEp.Key;
                    var epData = srEp.Value;

                    var epRequest = seasonRequests.Episodes.FirstOrDefault(x => x.EpisodeNumber == epNumber);
                    if (epRequest != null)
                    {
                        // We want to monior this episode since we have a request for it
                        // Let's check to see if it's wanted first, save an api call
                        if (epData.status.Equals(SickRageStatus.Wanted, StringComparison.CurrentCultureIgnoreCase))
                        {
                            continue;
                        }
                        var epResult = await SickRageApi.SetEpisodeStatus(settings.ApiKey, settings.FullUri, tvdbid,
                            SickRageStatus.Wanted, seasonRequests.SeasonNumber, epNumber);

                        Logger.LogDebug("Set the status to Wanted for Episode {0} in season {1}. The result is '{2}' : '{3}'", seasonRequests.SeasonNumber, epNumber, epResult.result, epResult.message);
                    }
                }
            }
            return true;
        }

        private async Task SearchForRequest(ChildRequests model, IEnumerable<Episode> sonarrEpList, SonarrSeries existingSeries, SonarrSettings s,
            IReadOnlyCollection<Episode> episodesToUpdate, IReadOnlyDictionary<int, int> seasonNumberMap)
        {
            foreach (var season in model.SeasonRequests)
            {
                var targetSeasonNumber = seasonNumberMap.TryGetValue(season.SeasonNumber, out var mappedSeason)
                    ? mappedSeason
                    : season.SeasonNumber;
                var sonarrSeason = sonarrEpList.Where(x => x.seasonNumber == targetSeasonNumber);
                var sonarrEpCount = sonarrSeason.Count();
                var ourRequestCount = season.Episodes.Count;

                // We have the same amount of requests as all of the episodes in the season,
                // or Sonarr has more episodes than Ombi (incomplete metadata).
                // Do a season search in both cases.
                if (sonarrEpCount >= ourRequestCount)
                {
                    await SonarrApi.SeasonSearch(existingSeries.id, targetSeasonNumber, s.ApiKey, s.FullUri);
                }
                else
                {
                    var requestedEpisodeIds = episodesToUpdate
                        .Where(x => x.seasonNumber == targetSeasonNumber)
                        .Select(x => x.id)
                        .Distinct()
                        .ToArray();
                    if (requestedEpisodeIds.Any())
                    {
                        await SonarrApi.EpisodeSearch(requestedEpisodeIds, s.ApiKey, s.FullUri);
                    }
                }
            }
        }

        private async Task<string> GetSonarrRootPath(int pathId, SonarrSettings sonarrSettings)
        {
            Logger.LogInformation("Getting Sonarr root path for ID: {PathId}", pathId);
            var rootFoldersResult = await SonarrApi.GetRootFolders(sonarrSettings.ApiKey, sonarrSettings.FullUri);
            
            if (rootFoldersResult == null || !rootFoldersResult.Any())
            {
                Logger.LogError("No root folders returned from Sonarr API");
                return string.Empty;
            }

            Logger.LogInformation("Found {Count} root folders in Sonarr", rootFoldersResult.Count());
            foreach (var folder in rootFoldersResult)
            {
                Logger.LogDebug("Root folder - ID: {Id}, Path: {Path}", folder.id, folder.path);
            }

            if (pathId == 0)
            {
                var defaultPath = rootFoldersResult.FirstOrDefault()?.path;
                Logger.LogInformation("Using first root folder as default: {Path}", defaultPath);
                return defaultPath;
            }

            var matchingFolder = rootFoldersResult.FirstOrDefault(r => r.id == pathId);
            if (matchingFolder != null)
            {
                Logger.LogInformation("Found matching root folder for ID {PathId}: {Path}", pathId, matchingFolder.path);
                return matchingFolder.path;
            }

            Logger.LogError("No matching root folder found for ID: {PathId}", pathId);
            return string.Empty;
        }

        private async Task AddToRequestFailureQueue(ChildRequests model, string errorMessage)
        {
            var existingQueue = await _requestQueueRepository.FirstOrDefaultAsync(x => x.RequestId == model.Id && x.Type == RequestType.TvShow);
            if (existingQueue != null)
            {
                existingQueue.RetryCount++;
                existingQueue.Error = errorMessage;
                await _requestQueueRepository.SaveChangesAsync();
            }
            else
            {
                await _requestQueueRepository.Add(new RequestQueue
                {
                    Dts = DateTime.UtcNow,
                    Error = errorMessage,
                    RequestId = model.Id,
                    Type = RequestType.TvShow,
                    RetryCount = 0
                });
                await _notificationHelper.Notify(model, NotificationType.ItemAddedToFaultQueue);
            }
        }
    }
}
