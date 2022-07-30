﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Search
{
    public class PlexAvailabilityRule : BaseSearchRule, IRules<SearchViewModel>
    {
        private readonly ISettingsService<PlexSettings> _plexSettings;
        private readonly IFeatureService _featureService;

        public PlexAvailabilityRule(IPlexContentRepository repo, ILogger<PlexAvailabilityRule> log, ISettingsService<PlexSettings> plexSettings,
            IFeatureService featureService)
        {
            PlexContentRepository = repo;
            Log = log;
            _plexSettings = plexSettings;
            _featureService = featureService;
        }

        private IPlexContentRepository PlexContentRepository { get; }
        private ILogger Log { get; }

        public async Task<RuleResult> Execute(SearchViewModel obj)
        {
            PlexServerContent item = null;
            var useImdb = false;
            var useTheMovieDb = false;
            var useId = false;
            var useTvDb = false;

            MediaType type = ConvertType(obj.Type);

            if (obj.ImdbId.HasValue())
            {
                item = await PlexContentRepository.GetByType(obj.ImdbId, ProviderType.ImdbId, type);
                if (item != null)
                {
                    useImdb = true;
                }
            }
            if (item == null)
            {
                if (obj.Id > 0)
                {
                    item = await PlexContentRepository.GetByType(obj.Id.ToString(), ProviderType.TheMovieDbId, type);
                    if (item != null)
                    {
                        useId = true;
                    }
                }
                if (obj.TheMovieDbId.HasValue())
                {
                    item = await PlexContentRepository.GetByType(obj.TheMovieDbId, ProviderType.TheMovieDbId, type);
                    if (item != null)
                    {
                        useTheMovieDb = true;
                    }
                }

                if (item == null)
                {
                    if (obj.TheTvDbId.HasValue())
                    {
                        item = await PlexContentRepository.GetByType(obj.TheTvDbId, ProviderType.TvDbId, type);
                        if (item != null)
                        {
                            useTvDb = true;
                        }
                    }
                }
            }

            if (item != null)
            {
                var settings = await _plexSettings.GetSettingsAsync();
                var firstServer = settings.Servers.FirstOrDefault();
                var host = string.Empty;
                if (firstServer != null)
                {
                    host = firstServer.ServerHostname;
                }
                if (useId)
                {
                    obj.TheMovieDbId = obj.Id.ToString();
                    useTheMovieDb = true;
                }

                if (obj is SearchMovieViewModel movie)
                {
                    var is4kEnabled = await _featureService.FeatureEnabled(FeatureNames.Movie4KRequests);

                    if (item.Has4K && is4kEnabled)
                    {
                        movie.Available4K = true;
                    }
                    else
                    {
                        obj.Available = true;
                        obj.Quality = item.Quality;
                    }

                    if (item.Quality.HasValue())
                    {
                        obj.Available = true;
                        obj.Quality = item.Quality;
                    }
                }
                else
                {
                    obj.Available = true;
                }

                if (item.Url.StartsWith("http"))
                {
                    obj.PlexUrl = item.Url;
                }
                else
                {
                    // legacy content
                    obj.PlexUrl = PlexHelper.BuildPlexMediaUrl(item.Url, host);
                }

                if (obj is SearchTvShowViewModel search)
                {
                    // Let's go through the episodes now
                    if (search.SeasonRequests.Any())
                    {
                        var allEpisodes = PlexContentRepository.GetAllEpisodes();
                        foreach (var season in search.SeasonRequests.ToList())
                        {
                            foreach (var episode in season.Episodes.ToList())
                            {
                                await AvailabilityRuleHelper.SingleEpisodeCheck(useImdb, allEpisodes, episode, season, item, useTheMovieDb, useTvDb, Log);
                            }
                        }

                        AvailabilityRuleHelper.CheckForUnairedEpisodes(search);
                    }
                }
            }
            return Success();
        }

        private MediaType ConvertType(RequestType type) =>
            type switch
            {
                RequestType.Movie => MediaType.Movie,
                RequestType.TvShow => MediaType.Series,
                _ => MediaType.Movie,
            };
    }
}