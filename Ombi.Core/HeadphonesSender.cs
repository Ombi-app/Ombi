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
using Ombi.Api.Interfaces;
using Ombi.Core.SettingModels;
using Ombi.Store;

namespace Ombi.Core
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
            var albumResult = await Api.AddAlbum(Settings.ApiKey, Settings.FullUri, request.ReleaseId);
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
            bool updated = RequestService.UpdateRequest(request);

            return updated;
        }

        private async Task<bool> AddArtist(RequestedModel request)
        {
            var index = await Api.GetIndex(Settings.ApiKey, Settings.FullUri);
            var artistExists = index.Any(x => x.ArtistID == request.ArtistId);
            bool artistAdd = false;
            if (!artistExists)
            {
                artistAdd = await Api.AddArtist(Settings.ApiKey, Settings.FullUri, request.ArtistId);
                Log.Info("Artist add result for {1}: {0}", artistAdd, request.ArtistName);
            }

            return artistExists || artistAdd;
        }

        private async Task<bool> SetAlbumStatus(RequestedModel request)
        {
            bool setStatus = await Api.QueueAlbum(Settings.ApiKey, Settings.FullUri, request.ReleaseId);
            return setStatus;
        }
    }
}