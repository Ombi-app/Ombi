using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Api.TvMaze;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TvMaze.Models;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Helpers
{
    public class TvShowRequestBuilder
    {

        public TvShowRequestBuilder(ITvMazeApi tvApi, IMovieDbApi movApi)
        {
            TvApi = tvApi;
            MovieDbApi = movApi;
        }
        
        private ITvMazeApi TvApi { get; }
        private IMovieDbApi MovieDbApi { get; }

        public ChildRequests ChildRequest { get; set; }
        public List<SeasonsViewModel> TvRequests { get; protected set; }
        public string PosterPath { get; protected set; }
        public string BackdropPath { get; protected set; }
        public DateTime FirstAir { get; protected set; }
        public TvRequests NewRequest { get; protected set; }
        protected TvMazeShow ShowInfo { get; set; }
        protected List<TvSearchResult> Results { get; set; }

        public async Task<TvShowRequestBuilder> GetShowInfo(int id)
        {
            ShowInfo = await TvApi.ShowLookupByTheTvDbId(id);
            Results = await MovieDbApi.SearchTv(ShowInfo.name);
            foreach (TvSearchResult result in Results) {
                if (result.Name.Equals(ShowInfo.name, StringComparison.InvariantCultureIgnoreCase))
                {                  
                    var showIds = await MovieDbApi.GetTvExternals(result.Id);
                    ShowInfo.externals.imdb = showIds.imdb_id;
                    BackdropPath = result.BackdropPath;
                    break;
                }
            }

            DateTime.TryParse(ShowInfo.premiered, out var dt);

            FirstAir = dt;

            // For some reason the poster path is always http
            PosterPath = ShowInfo.image?.medium.ToHttpsUrl();

            return this;
        }
        
        public TvShowRequestBuilder CreateChild(TvRequestViewModel model, string userId)
        {
            ChildRequest = new ChildRequests
            {
                Id = model.TvDbId, // This is set to 0 after the request rules have run, the request rules needs it to identify the request
                RequestType = RequestType.TvShow,
                RequestedDate = DateTime.UtcNow,
                Approved = false,
                RequestedUserId = userId,
                SeasonRequests = new List<SeasonRequests>(),
                Title = ShowInfo.name,
                ReleaseYear = FirstAir,
                RequestedByAlias = model.RequestedByAlias,
                SeriesType = ShowInfo.genres.Any( s => s.Equals("Anime", StringComparison.InvariantCultureIgnoreCase)) ? SeriesType.Anime : SeriesType.Standard
            };

            return this;
        }

        public TvShowRequestBuilder CreateTvList(TvRequestViewModel tv)
        {
            TvRequests = new List<SeasonsViewModel>();
            // Only have the TV requests we actually requested and not everything
            foreach (var season in tv.Seasons)
            {
                if (season.Episodes.Any())
                {
                    TvRequests.Add(season);
                }
            }

            return this;
        }


        public async Task<TvShowRequestBuilder> BuildEpisodes(TvRequestViewModel tv)
        {
            if (tv.RequestAll)
            {
                var episodes = await TvApi.EpisodeLookup(ShowInfo.id);
                foreach (var ep in episodes)
                {
                    var season = ChildRequest.SeasonRequests.FirstOrDefault(x => x.SeasonNumber == ep.season);
                    if (season == null)
                    {
                        ChildRequest.SeasonRequests.Add(new SeasonRequests
                        {
                            Episodes = new List<EpisodeRequests>{
                                new EpisodeRequests
                                {
                                    EpisodeNumber = ep.number,
                                    AirDate = FormatDate(ep.airdate),
                                    Title = ep.name,
                                    Url = ep.url.ToHttpsUrl()
                                }
                            },
                            SeasonNumber = ep.season,
                        });
                    }
                    else
                    {
                        season.Episodes.Add(new EpisodeRequests
                        {
                            EpisodeNumber = ep.number,
                            AirDate = FormatDate(ep.airdate),
                            Title = ep.name,
                            Url = ep.url.ToHttpsUrl()
                        });
                    }
                }

            }
            else if (tv.LatestSeason)
            {
                var episodes = await TvApi.EpisodeLookup(ShowInfo.id);
                var latest = episodes.OrderByDescending(x => x.season).FirstOrDefault();
                var episodesRequests = new List<EpisodeRequests>();
                foreach (var ep in episodes)
                {
                    if (ep.season == latest.season)
                    {
                        episodesRequests.Add(new EpisodeRequests
                        {
                            EpisodeNumber = ep.number,
                            AirDate = FormatDate(ep.airdate),
                            Title = ep.name,
                            Url = ep.url.ToHttpsUrl()
                        });
                    }
                }
                ChildRequest.SeasonRequests.Add(new SeasonRequests
                {
                    Episodes = episodesRequests,
                    SeasonNumber = latest.season,
                });
            }
            else if (tv.FirstSeason)
            {
                var episodes = await TvApi.EpisodeLookup(ShowInfo.id);
                var first = episodes.OrderBy(x => x.season).FirstOrDefault();
                var episodesRequests = new List<EpisodeRequests>();
                foreach (var ep in episodes)
                {
                    if (ep.season == first.season)
                    {
                        episodesRequests.Add(new EpisodeRequests
                        {
                            EpisodeNumber = ep.number,
                            AirDate = FormatDate(ep.airdate),
                            Title = ep.name,
                            Url = ep.url.ToHttpsUrl()
                        });
                    }
                }
                ChildRequest.SeasonRequests.Add(new SeasonRequests
                {
                    Episodes = episodesRequests,
                    SeasonNumber = first.season,
                });
            }
            else
            {
                // It's a custom request
                var seasonRequests = new List<SeasonRequests>();
                var episodes = await TvApi.EpisodeLookup(ShowInfo.id);
                foreach (var ep in episodes)
                {
                    var existingSeasonRequest = seasonRequests.FirstOrDefault(x => x.SeasonNumber == ep.season);
                    if (existingSeasonRequest != null)
                    {
                        var requestedSeason = tv.Seasons.FirstOrDefault(x => x.SeasonNumber == ep.season);
                        var requestedEpisode = requestedSeason?.Episodes?.Any(x => x.EpisodeNumber == ep.number) ?? false;
                        if (requestedSeason != null && requestedEpisode)
                        {
                            // We already have this, let's just add the episodes to it
                            existingSeasonRequest.Episodes.Add(new EpisodeRequests
                            {
                                EpisodeNumber = ep.number,
                                AirDate = FormatDate(ep.airdate),
                                Title = ep.name,
                                Url = ep.url.ToHttpsUrl()
                            });
                        }
                    }
                    else
                    {
                        var newRequest = new SeasonRequests {SeasonNumber = ep.season};
                        var requestedSeason = tv.Seasons.FirstOrDefault(x => x.SeasonNumber == ep.season);
                        var requestedEpisode = requestedSeason?.Episodes?.Any(x => x.EpisodeNumber == ep.number) ?? false;
                        if (requestedSeason != null && requestedEpisode)
                        {
                            newRequest.Episodes.Add(new EpisodeRequests
                            {
                                EpisodeNumber = ep.number,
                                AirDate = FormatDate(ep.airdate),
                                Title = ep.name,
                                Url = ep.url.ToHttpsUrl()
                            });
                            seasonRequests.Add(newRequest);
                        }
                    }
                }

                foreach (var s in seasonRequests)
                {
                    ChildRequest.SeasonRequests.Add(s);
                }
            }
            return this;
        }
        
        
        public TvShowRequestBuilder CreateNewRequest(TvRequestViewModel tv)
        {
            NewRequest = new TvRequests
            {
                Overview = ShowInfo.summary.RemoveHtml(),
                PosterPath = PosterPath,
                Title = ShowInfo.name,
                ReleaseDate = FirstAir,
                Status = ShowInfo.status,
                ImdbId = ShowInfo.externals?.imdb ?? string.Empty,
                TvDbId = tv.TvDbId,
                ChildRequests = new List<ChildRequests>(),
                TotalSeasons = tv.Seasons.Count(),
                Background = BackdropPath
            };
            NewRequest.ChildRequests.Add(ChildRequest);

            return this;
        }

        private DateTime FormatDate(string date)
        {
            return string.IsNullOrEmpty(date) ? DateTime.MinValue : DateTime.Parse(date);
        }
    }
}