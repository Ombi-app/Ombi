using Ombi.Api.RottenTomatoes.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ombi.Api.RottenTomatoes
{
    public interface IRottenTomatoesApi
    {
        Task<MovieRatings> GetMovieRatings(string movieName, int movieYear);
        Task<TvRatings> GetTvRatings(string showName, int showYear);
    }
}
