#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: JellyfinSystemInfo.cs
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
    public class JellyfinSystemInfo
    {
        public string SystemUpdateLevel { get; set; }
        public string OperatingSystemDisplayName { get; set; }
        public bool SupportsRunningAsService { get; set; }
        public string MacAddress { get; set; }
        public bool HasPendingRestart { get; set; }
        public bool SupportsLibraryMonitor { get; set; }
        public object[] InProgressInstallations { get; set; }
        public int WebSocketPortNumber { get; set; }
        public object[] CompletedInstallations { get; set; }
        public bool CanSelfRestart { get; set; }
        public bool CanSelfUpdate { get; set; }
        public object[] FailedPluginAssemblies { get; set; }
        public string ProgramDataPath { get; set; }
        public string ItemsByNamePath { get; set; }
        public string CachePath { get; set; }
        public string LogPath { get; set; }
        public string InternalMetadataPath { get; set; }
        public string TranscodingTempPath { get; set; }
        public int HttpServerPortNumber { get; set; }
        public bool SupportsHttps { get; set; }
        public int HttpsPortNumber { get; set; }
        public bool HasUpdateAvailable { get; set; }
        public bool SupportsAutoRunAtStartup { get; set; }
        public string EncoderLocationType { get; set; }
        public string SystemArchitecture { get; set; }
        public string LocalAddress { get; set; }
        public string WanAddress { get; set; }
        public string ServerName { get; set; }
        public string Version { get; set; }
        public string OperatingSystem { get; set; }
        public string Id { get; set; }
    }
}