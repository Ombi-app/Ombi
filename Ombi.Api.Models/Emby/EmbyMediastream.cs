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
    public class EmbyMediastream
    {
        public string Codec { get; set; }
        public string Language { get; set; }
        public string TimeBase { get; set; }
        public string CodecTimeBase { get; set; }
        public string NalLengthSize { get; set; }
        public bool IsInterlaced { get; set; }
        public bool IsAVC { get; set; }
        public int BitRate { get; set; }
        public int BitDepth { get; set; }
        public int RefFrames { get; set; }
        public bool IsDefault { get; set; }
        public bool IsForced { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public float AverageFrameRate { get; set; }
        public float RealFrameRate { get; set; }
        public string Profile { get; set; }
        public string Type { get; set; }
        public string AspectRatio { get; set; }
        public int Index { get; set; }
        public bool IsExternal { get; set; }
        public bool IsTextSubtitleStream { get; set; }
        public bool SupportsExternalStream { get; set; }
        public string PixelFormat { get; set; }
        public int Level { get; set; }
        public bool IsAnamorphic { get; set; }
        public string DisplayTitle { get; set; }
        public string ChannelLayout { get; set; }
        public int Channels { get; set; }
        public int SampleRate { get; set; }
    }
}