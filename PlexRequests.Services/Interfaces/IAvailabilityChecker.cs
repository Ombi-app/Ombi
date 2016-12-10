#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: IAvailabilityChecker.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion
using PlexRequests.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

using PlexRequests.Store.Models;
using PlexRequests.Store.Models.Plex;

namespace PlexRequests.Services.Interfaces
{
    public interface IAvailabilityChecker
    {
        void CheckAndUpdateAll();
        IEnumerable<PlexContent> GetPlexMovies(IEnumerable<PlexContent> content);
        bool IsMovieAvailable(PlexContent[] plexMovies, string title, string year, string providerId = null);
        IEnumerable<PlexContent> GetPlexTvShows(IEnumerable<PlexContent> content);
        bool IsTvShowAvailable(PlexContent[] plexShows, string title, string year, string providerId = null, int[] seasons = null);
        IEnumerable<PlexContent> GetPlexAlbums(IEnumerable<PlexContent> content);
        bool IsAlbumAvailable(PlexContent[] plexAlbums, string title, string year, string artist);
        bool IsEpisodeAvailable(string theTvDbId, int season, int episode);
        PlexContent GetAlbum(PlexContent[] plexAlbums, string title, string year, string artist);
        PlexContent GetMovie(PlexContent[] plexMovies, string title, string year, string providerId = null);
        PlexContent GetTvShow(PlexContent[] plexShows, string title, string year, string providerId = null, int[] seasons = null);
        /// <summary>
        /// Gets the episode's stored in the cache.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<PlexEpisodes>> GetEpisodes();
        /// <summary>
        /// Gets the episode's stored in the cache and then filters on the TheTvDBId.
        /// </summary>
        /// <param name="theTvDbId">The tv database identifier.</param>
        /// <returns></returns>
        Task<IEnumerable<PlexEpisodes>> GetEpisodes(int theTvDbId);
    }
}