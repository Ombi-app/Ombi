#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: IPlexApi.cs
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
using System;

using PlexRequests.Api.Models.Plex;

namespace PlexRequests.Api.Interfaces
{
    public interface IPlexApi
    {
        PlexAuthentication SignIn(string username, string password);
        PlexFriends GetUsers(string authToken);
        PlexSearch SearchContent(string authToken, string searchTerm, Uri plexFullHost);
        PlexStatus GetStatus(string authToken, Uri uri);
        PlexAccount GetAccount(string authToken);
        PlexLibraries GetLibrarySections(string authToken, Uri plexFullHost);
        PlexSearch GetLibrary(string authToken, Uri plexFullHost, string libraryId);
        PlexMetadata GetMetadata(string authToken, Uri plexFullHost, string itemId);
        PlexEpisodeMetadata GetEpisodeMetaData(string authToken, Uri host, string ratingKey);
        PlexSearch GetAllEpisodes(string authToken, Uri host, string section, int startPage, int returnCount);
        PlexServer GetServer(string authToken);
        PlexSeasonMetadata GetSeasons(string authToken, Uri plexFullHost, string ratingKey);
        RecentlyAdded RecentlyAdded(string authToken, Uri plexFullHost);
    }
}