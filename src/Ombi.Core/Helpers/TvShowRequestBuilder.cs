using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Api.TvMaze;
using Ombi.Api.TvMaze.Models;
using Ombi.Core.Models.Search;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Helpers
{
    public class TvShowRequestBuilder
    {

        public TvShowRequestBuilder(ITvMazeApi tvApi)
        {
            TvApi = tvApi;
        }
        
        private ITvMazeApi TvApi { get; }

        public ChildRequests ChildRequest { get; set; }
        public List<SeasonRequests> TvRequests { get; protected set; }
        public string PosterPath { get; protected set; }
        public DateTime FirstAir { get; protected set; }
        public TvRequests NewRequest { get; protected set; }
        protected TvMazeShow ShowInfo { get; set; }

        public async Task<TvShowRequestBuilder> GetShowInfo(int id)
        {
            ShowInfo = await TvApi.ShowLookupByTheTvDbId(id);

            DateTime.TryParse(ShowInfo.premiered, out DateTime dt);

            FirstAir = dt;

            // For some reason the poster path is always http
            PosterPath = ShowInfo.image?.medium.Replace("http:", "https:");

            return this;
        }
        
        public TvShowRequestBuilder CreateChild(SearchTvShowViewModel model, string userId)
        {
            ChildRequest = new ChildRequests
            {
                Id = model.Id,
                RequestType = RequestType.TvShow,
                RequestedDate = DateTime.UtcNow,
                Approved = false,
                RequestedUserId = userId,
                SeasonRequests = new List<SeasonRequests>(),
                Title = model.Title,
                SeriesType = ShowInfo.type.Equals("Animation", StringComparison.CurrentCultureIgnoreCase) ? SeriesType.Anime : SeriesType.Standard
            };

            return this;
        }

        public TvShowRequestBuilder CreateTvList(SearchTvShowViewModel tv)
        {
            TvRequests = new List<SeasonRequests>();
            // Only have the TV requests we actually requested and not everything
            foreach (var season in tv.SeasonRequests)
            {
                for (int i = season.Episodes.Count - 1; i >= 0; i--)
                {
                    if (!season.Episodes[i].Requested)
                    {
                        season.Episodes.RemoveAt(i); // Remove the episode since it's not requested
                    }
                }

                if (season.Episodes.Any())
                {
                    TvRequests.Add(season);
                }
            }

            return this;

        }


        public async Task<TvShowRequestBuilder> BuildEpisodes(SearchTvShowViewModel tv)
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
                                    Url = ep.url
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
                            Url = ep.url
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
                            Url = ep.url
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
                            Url = ep.url
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
                ChildRequest.SeasonRequests = TvRequests;
            }
            return this;
        }
        
        
        public TvShowRequestBuilder CreateNewRequest(SearchTvShowViewModel tv)
        {
            NewRequest = new TvRequests
            {
                Id = tv.Id,
                Overview = ShowInfo.summary.RemoveHtml(),
                PosterPath = PosterPath,
                Title = ShowInfo.name,
                ReleaseDate = FirstAir,
                Status = ShowInfo.status,
                ImdbId = ShowInfo.externals?.imdb ?? string.Empty,
                TvDbId = tv.Id,
                ChildRequests = new List<ChildRequests>(),
                TotalSeasons = tv.SeasonRequests.Count()
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