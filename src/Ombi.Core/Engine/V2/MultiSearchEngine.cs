using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Ombi.Api.MusicBrainz;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search.V2;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Engine.V2
{
    public class MultiSearchEngine : BaseMediaEngine, IMultiSearchEngine
    {
        public MultiSearchEngine(IPrincipal identity, IRequestServiceMain requestService, IRuleEvaluator rules,
            OmbiUserManager um, ICacheService cache, ISettingsService<OmbiSettings> ombiSettings, IRepository<RequestSubscription> sub,
            IMovieDbApi movieDbApi, ISettingsService<LidarrSettings> lidarrSettings, IMusicBrainzApi musicApi)
            : base(identity, requestService, rules, um, cache, ombiSettings, sub)
        {
            _movieDbApi = movieDbApi;
            _lidarrSettings = lidarrSettings;
            _musicApi = musicApi;
        }

        private readonly IMovieDbApi _movieDbApi;
        private readonly ISettingsService<LidarrSettings> _lidarrSettings;
        private readonly IMusicBrainzApi _musicApi;


        public async Task<List<MultiSearchResult>> MultiSearch(string searchTerm, MultiSearchFilter filter, CancellationToken cancellationToken)
        {
            var lang = await DefaultLanguageCode(null);
            var model = new List<MultiSearchResult>();

            var movieDbData = (await _movieDbApi.MultiSearch(searchTerm, lang, cancellationToken)).results;

            var lidarrSettings = await _lidarrSettings.GetSettingsAsync();
            if (lidarrSettings.Enabled && filter.Music)
            {
                var artistResult = await _musicApi.SearchArtist(searchTerm);
                foreach (var artist in artistResult)
                {
                    model.Add(new MultiSearchResult
                    {
                        MediaType = "Artist",
                        Title = artist.Name,
                        Id = artist.Id
                    });
                }
            }

            foreach (var multiSearch in movieDbData)
            {
                var result = new MultiSearchResult
                {
                    MediaType = multiSearch.media_type,
                    Poster = multiSearch.poster_path
                };

                if (multiSearch.media_type.Equals("movie", StringComparison.InvariantCultureIgnoreCase) && filter.Movies)
                {
                    if (multiSearch.release_date.HasValue() && DateTime.TryParse(multiSearch.release_date, out var releaseDate))
                    {
                        result.Title = $"{multiSearch.title} ({releaseDate.Year})";
                    }
                    else
                    {
                        result.Title = multiSearch.title;
                    }
                }

                else if (multiSearch.media_type.Equals("tv", StringComparison.InvariantCultureIgnoreCase) && filter.TvShows)
                {
                    if (multiSearch.release_date.HasValue() && DateTime.TryParse(multiSearch.release_date, out var releaseDate))
                    {
                        result.Title = $"{multiSearch.name} ({releaseDate.Year})";
                    }
                    else
                    {
                        result.Title = multiSearch.name;
                    }
                }
                else if (multiSearch.media_type.Equals("person", StringComparison.InvariantCultureIgnoreCase) && filter.People)
                {
                    result.Title = multiSearch.name;
                }
                else
                {
                    continue;
                }

                result.Id = multiSearch.id.ToString();
                model.Add(result);
            }

            return model;
        }
    }
}
