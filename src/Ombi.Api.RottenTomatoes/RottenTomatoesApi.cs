using Ombi.Api.RottenTomatoes.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ombi.Api.RottenTomatoes
{
    public class RottenTomatoesApi : IRottenTomatoesApi
    {
        public RottenTomatoesApi(IApi api)
        {
            _api = api;
        }

        private string Endpoint => "https://www.rottentomatoes.com/api/private";
        private IApi _api { get; }

        public async Task<MovieRatings> GetMovieRatings(string movieName, int movieYear)
        {
            var request = new Request("/v1.0/movies", Endpoint, HttpMethod.Get);
            request.AddHeader("Accept", "application/json");
            request.AddQueryString("q", movieName);
            var result = await _api.Request<RottenTomatoesMovieResponse>(request);

            var movieFound = result.movies.FirstOrDefault(x => x.year == movieYear);
            if (movieFound == null)
            {
                return null;
            }

            return movieFound.ratings;
        }

        public async Task<TvRatings> GetTvRatings(string showName, int showYear)
        {
            var request = new Request("/v2.0/search/", Endpoint, HttpMethod.Get);
            request.AddHeader("Accept", "application/json");
            request.AddQueryString("q", showName);
            request.AddQueryString("limit", 10.ToString());
            var result = await _api.Request<RottenTomatoesTvResponse>(request);

            var showFound = result.tvSeries.FirstOrDefault(x => x.startYear == showYear);
            if (showFound == null)
            {
                return null;
            }

            return new TvRatings
            {
                Class = showFound.meterClass,
                Score = showFound.meterScore
            };
        }
    }
}
