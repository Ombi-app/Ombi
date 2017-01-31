#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: MovieInformation.cs
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


namespace Ombi.Api.Models.Emby
{
    public class EmbyMediasource
    {
        public string Protocol { get; set; }
        public string Id { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public string Container { get; set; }
        public string Name { get; set; }
        public bool IsRemote { get; set; }
        public string ETag { get; set; }
        public long RunTimeTicks { get; set; }
        public bool ReadAtNativeFramerate { get; set; }
        public bool SupportsTranscoding { get; set; }
        public bool SupportsDirectStream { get; set; }
        public bool SupportsDirectPlay { get; set; }
        public bool IsInfiniteStream { get; set; }
        public bool RequiresOpening { get; set; }
        public bool RequiresClosing { get; set; }
        public bool SupportsProbing { get; set; }
        public string VideoType { get; set; }
        public EmbyMediastream[] MediaStreams { get; set; }
        public object[] PlayableStreamFileNames { get; set; }
        public object[] Formats { get; set; }
        public int Bitrate { get; set; }
        public EmbyRequiredhttpheaders RequiredHttpHeaders { get; set; }
        public int DefaultAudioStreamIndex { get; set; }

    }
}