#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: JellyfinPolicy.cs
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
namespace Ombi.Api.Jellyfin.Models
{
    public class JellyfinPolicy
    {
        public bool IsAdministrator { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDisabled { get; set; }
        public object[] BlockedTags { get; set; }
        public bool EnableUserPreferenceAccess { get; set; }
        public object[] AccessSchedules { get; set; }
        public object[] BlockUnratedItems { get; set; }
        public bool EnableRemoteControlOfOtherUsers { get; set; }
        public bool EnableSharedDeviceControl { get; set; }
        public bool EnableLiveTvManagement { get; set; }
        public bool EnableLiveTvAccess { get; set; }
        public bool EnableMediaPlayback { get; set; }
        public bool EnableAudioPlaybackTranscoding { get; set; }
        public bool EnableVideoPlaybackTranscoding { get; set; }
        public bool EnablePlaybackRemuxing { get; set; }
        public bool EnableContentDeletion { get; set; }
        public bool EnableContentDownloading { get; set; }
        public bool EnableSync { get; set; }
        public bool EnableSyncTranscoding { get; set; }
        public object[] EnabledDevices { get; set; }
        public bool EnableAllDevices { get; set; }
        public object[] EnabledChannels { get; set; }
        public bool EnableAllChannels { get; set; }
        public object[] EnabledFolders { get; set; }
        public bool EnableAllFolders { get; set; }
        public int InvalidLoginAttemptCount { get; set; }
        public bool EnablePublicSharing { get; set; }
    }
}