﻿using Microsoft.AspNetCore.Mvc;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;

// Due to conflicting Genre models in
// Ombi.TheMovieDbApi.Models and Ombi.Api.TheMovieDb.Models   
using Genre = Ombi.TheMovieDbApi.Models.Genre;

namespace Ombi.Controllers.External
{
    [Admin]
    [ApiV1]
    [Produces("application/json")]
    public sealed class TheMovieDbController : Controller
    {
        public TheMovieDbController(IMovieDbApi tmdbApi) => TmdbApi = tmdbApi;

        private IMovieDbApi TmdbApi { get; }

        /// <summary>
        /// Searches for keywords matching the specified term.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        [HttpGet("Keywords")]
        public async Task<IEnumerable<Keyword>> GetKeywords([FromQuery]string searchTerm) =>
            await TmdbApi.SearchKeyword(searchTerm);

        /// <summary>
        /// Gets the keyword matching the specified ID.
        /// </summary>
        /// <param name="keywordId">The keyword ID.</param>
        [HttpGet("Keywords/{keywordId}")]
        public async Task<IActionResult> GetKeywords(int keywordId)
        {
            var keyword = await TmdbApi.GetKeyword(keywordId);
            return keyword == null ? NotFound() : (IActionResult)Ok(keyword);
        }

        /// <summary>
        /// Gets the genres for either Tv or Movies depending on media type
        /// </summary>
        /// <param name="media">Either `tv` or `movie`.</param> 
        [HttpGet("Genres/{media}")]
        public async Task<IEnumerable<Genre>> GetGenres(string media) =>
            await TmdbApi.GetGenres(media);
    }
}
