#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: HeadphonesSender.cs
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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using PlexRequests.Api.Interfaces;
using PlexRequests.Core.SettingModels;
using PlexRequests.Store;

namespace PlexRequests.Core
{
    public class HeadphonesSender
    {
        public HeadphonesSender(IHeadphonesApi api, HeadphonesSettings settings, IRequestService request)
        {
            Api = api;
            Settings = settings;
            RequestService = request;
        }

        private int WaitTime => 2000;
        private int CounterMax => 60;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private IHeadphonesApi Api { get; }
        private IRequestService RequestService { get; }
        private HeadphonesSettings Settings { get; }

        public async Task<bool> AddAlbum(RequestedModel request)
        {
            var addArtistResult = await AddArtist(request);
            if (!addArtistResult)
            {
                return false;
            }

            // Artist is now active
            // Add album
            var albumResult = await Api.AddAlbum(Settings.ApiKey, Settings.FullUri, request.MusicBrainzId);
            if (!albumResult)
            {
                Log.Error("Couldn't add the album to headphones");
            }

            // Set the status to wanted and search
            var status = await SetAlbumStatus(request);
            if (!status)
            {
                return false;
            }

            // Approve it
            request.Approved = true;

            // Update the record
            var updated = RequestService.UpdateRequest(request);

            return updated;
        }

        private async Task<bool> AddArtist(RequestedModel request)
        {
            var index = await Api.GetIndex(Settings.ApiKey, Settings.FullUri);
            var artistExists = index.Any(x => x.ArtistID == request.ArtistId);
            if (!artistExists)
            {
                var artistAdd = Api.AddArtist(Settings.ApiKey, Settings.FullUri, request.ArtistId);
                Log.Info("Artist add result : {0}", artistAdd);
            }

            var counter = 0;
            while (index.All(x => x.ArtistID != request.ArtistId))
            {
                Thread.Sleep(WaitTime);
                counter++;
                Log.Trace("Artist is still not present in the index. Counter = {0}", counter);
                index = await Api.GetIndex(Settings.ApiKey, Settings.FullUri);

                if (counter > CounterMax)
                {
                    Log.Trace("Artist is still not present in the index. Counter = {0}. Returning false", counter);
                    Log.Warn("We have tried adding the artist but it seems they are still not in headphones.");
                    return false;
                }
            }
            
            counter = 0;
            var artistStatus = index.Where(x => x.ArtistID == request.ArtistId).Select(x => x.Status).FirstOrDefault();
            while (artistStatus != "Active")
            {
                Thread.Sleep(WaitTime);
                counter++;
                Log.Trace("Artist status {1}. Counter = {0}", counter, artistStatus);
                index = await Api.GetIndex(Settings.ApiKey, Settings.FullUri);
                artistStatus = index.Where(x => x.ArtistID == request.ArtistId).Select(x => x.Status).FirstOrDefault();
                if (counter > CounterMax)
                {
                    Log.Trace("Artist status is still not active. Counter = {0}. Returning false", counter);
                    Log.Warn("The artist status is still not Active. We have waited long enough, seems to be a big delay in headphones.");
                    return false;
                }
            }

            var addedArtist = index.FirstOrDefault(x => x.ArtistID == request.ArtistId);
            var artistName = addedArtist?.ArtistName ?? string.Empty;
            counter = 0;
            while (artistName.Contains("Fetch failed"))
            {
                Thread.Sleep(WaitTime);
                await Api.RefreshArtist(Settings.ApiKey, Settings.FullUri, request.ArtistId);

                index = await Api.GetIndex(Settings.ApiKey, Settings.FullUri);

                artistName = index?.FirstOrDefault(x => x.ArtistID == request.ArtistId)?.ArtistName ?? string.Empty;
                counter++;
                if (counter > CounterMax)
                {
                    Log.Trace("Artist fetch has failed. Counter = {0}. Returning false", counter);
                    Log.Warn("Artist in headphones fetch has failed, we have tried refreshing the artist but no luck.");
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> SetAlbumStatus(RequestedModel request)
        {
            var counter = 0;
            var setStatus = await Api.QueueAlbum(Settings.ApiKey, Settings.FullUri, request.MusicBrainzId);

            while (!setStatus)
            {
                Thread.Sleep(WaitTime);
                counter++;
                Log.Trace("Setting Album status. Counter = {0}", counter);
                setStatus = await Api.QueueAlbum(Settings.ApiKey, Settings.FullUri, request.MusicBrainzId);
                if (counter > CounterMax)
                {
                    Log.Trace("Album status is still not active. Counter = {0}. Returning false", counter);
                    Log.Warn("We tried to se the status for the album but headphones didn't want to snatch it.");
                    return false;
                }
            }
            return true;
        }
    }
}