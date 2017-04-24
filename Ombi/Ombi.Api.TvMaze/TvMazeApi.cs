using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Ombi.Api.TvMaze.Models;

namespace Ombi.Api.TvMaze
{
    public class TvMazeApi : ITvMazeApi
    {
        public TvMazeApi()
        {
            Api = new Ombi.Api.Api();
            //Mapper = mapper;
        }
        private string Uri = "http://api.tvmaze.com";
        private Api Api { get; }
        public async Task<List<TvMazeSearch>> Search(string searchTerm)
        {
            var request = new Request("search/shows", Uri, HttpMethod.Get);

            request.AddQueryString("q", searchTerm);
            request.AddHeader("Content-Type", "application/json");

            return await Api.Request<List<TvMazeSearch>>(request);
        }

        public async Task<TvMazeShow> ShowLookup(int showId)
        {
            var request = new Request($"shows/{showId}", Uri, HttpMethod.Get);
            request.AddHeader("Content-Type", "application/json");

            return await Api.Request<TvMazeShow>(request);
        }

        public async Task<IEnumerable<TvMazeEpisodes>> EpisodeLookup(int showId)
        {

            var request = new Request($"shows/{showId}/episodes", Uri, HttpMethod.Get);

            request.AddHeader("Content-Type", "application/json");

            return await Api.Request<List<TvMazeEpisodes>>(request);
        }

        public async Task<TvMazeShow> ShowLookupByTheTvDbId(int theTvDbId)
        {
            var request = new Request($"lookup/shows?thetvdb={theTvDbId}", Uri, HttpMethod.Get);
            request.AddHeader("Content-Type", "application/json");
            try
            {
                var obj = await Api.Request<TvMazeShow>(request);

                var episodes = await EpisodeLookup(obj.id);

                foreach (var e in episodes)
                {
                    obj.Season.Add(new TvMazeCustomSeason
                    {
                        SeasonNumber = e.season,
                        EpisodeNumber = e.number
                    });
                }

                return obj;
            }
            catch (Exception e)
            {
                // TODO
                return null;
            }
        }

        public async Task<List<TvMazeSeasons>> GetSeasons(int id)
        {
            var request = new Request($"shows/{id}/seasons", Uri, HttpMethod.Get);

            request.AddHeader("Content-Type", "application/json");

            return await Api.Request<List<TvMazeSeasons>>(request);
        }

    }
}
