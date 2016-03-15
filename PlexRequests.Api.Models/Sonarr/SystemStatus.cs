#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: SystemStatus.cs
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
namespace PlexRequests.Api.Models.Sonarr
{
    public class SystemStatus
    {
        public string version { get; set; }
        public string buildTime { get; set; }
        public bool isDebug { get; set; }
        public bool isProduction { get; set; }
        public bool isAdmin { get; set; }
        public bool isUserInteractive { get; set; }
        public string startupPath { get; set; }
        public string appData { get; set; }
        public string osVersion { get; set; }
        public bool isMono { get; set; }
        public bool isLinux { get; set; }
        public bool isWindows { get; set; }
        public string branch { get; set; }
        public bool authentication { get; set; }
        public int startOfWeek { get; set; }
        public string urlBase { get; set; }
    }
}