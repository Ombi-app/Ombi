using System;
using System.Collections.Generic;
using Ombi.Api.Models.Radarr;
using Ombi.Api.Models.Sonarr;

namespace Ombi.Api.Interfaces
{
    public interface IRadarrApi
    {
        RadarrAddMovie AddMovie(int tmdbId, string title, int year, int qualityId, string rootPath, string apiKey, Uri baseUrl, bool searchNow = false);
        List<RadarrMovieResponse> GetMovies(string apiKey, Uri baseUrl);
        List<SonarrProfile> GetProfiles(string apiKey, Uri baseUrl);
        SystemStatus SystemStatus(string apiKey, Uri baseUrl);
        List<SonarrRootFolder> GetRootFolders(string apiKey, Uri baseUrl);
    }
}