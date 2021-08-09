using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Api.Lidarr;
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
            IMovieDbApi movieDbApi, ISettingsService<LidarrSettings> lidarrSettings, ILidarrApi lidarrApi)
            : base(identity, requestService, rules, um, cache, ombiSettings, sub)
        {
            _movieDbApi = movieDbApi;
            _lidarrSettings = lidarrSettings;
            _lidarrApi = lidarrApi;
        }

        private readonly IMovieDbApi _movieDbApi;
        private readonly ISettingsService<LidarrSettings> _lidarrSettings;

        private readonly ILidarrApi _lidarrApi;

        private bool _demo = DemoSingleton.Instance.Demo;


        public async Task<List<MultiSearchResult>> MultiSearch(string searchTerm, MultiSearchFilter filter, CancellationToken cancellationToken)
        {
            var lang = await DefaultLanguageCode(null);
            var model = new List<MultiSearchResult>();

            var lidarrSettings = await _lidarrSettings.GetSettingsAsync();
            if (lidarrSettings.Enabled && filter.Music)
            {
                var lidarSearchResult = await _lidarrApi.Search(searchTerm, lidarrSettings.ApiKey, lidarrSettings.FullUri);
                foreach (var search_result in lidarSearchResult)
                {
                    if (search_result.artist != null) 
                    {
                        model.Add(new MultiSearchResult
                        {
                            MediaType = "Artist",
                            Title = search_result.artist.artistName,
                            Id = search_result.artist.foreignArtistId,
                            Poster = search_result.artist.remotePoster,
                            Monitored = search_result.artist.monitored
                            
                        });
                    } else if (search_result.album != null)
                    {
                        model.Add(new MultiSearchResult
                        {
                            MediaType = "Album",
                            Title = search_result.album.title,
                            Id = search_result.album.foreignAlbumId,
                            Poster = search_result.album.remoteCover,
                            Monitored = search_result.album.monitored
                        });
                    }

                }
            }

            if (filter.Movies)
            {
                var movieDbData = (await _movieDbApi.MultiSearch(searchTerm, lang, cancellationToken)).results;

                foreach (var multiSearch in movieDbData)
                {

                    if (DemoCheck(multiSearch.title) || DemoCheck(multiSearch.name))
                    {
                        continue;
                    }

                    var result = new MultiSearchResult
                    {
                        MediaType = multiSearch.media_type,
                        Poster = multiSearch.poster_path,
                        Overview = multiSearch.overview
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
            }

            return model;
        }
    }
}
