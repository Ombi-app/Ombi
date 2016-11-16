#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: JobNames.cs
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
namespace PlexRequests.Services.Jobs
{
    public static class JobNames
    {
        public const string StoreBackup = "Database Backup";
        public const string CpCacher = "CouchPotato Cacher";
        public const string SonarrCacher = "Sonarr Cacher";
        public const string SrCacher = "SickRage Cacher";
        public const string PlexChecker = "Plex Availability Cacher";
        public const string PlexCacher = "Plex Cacher";
        public const string StoreCleanup = "Database Cleanup";
        public const string RequestLimitReset = "Request Limit Reset";
        public const string EpisodeCacher = "Plex Episode Cacher";
        public const string RecentlyAddedEmail = "Recently Added Email Notification";
        public const string FaultQueueHandler = "Request Fault Queue Handler";
    }
}