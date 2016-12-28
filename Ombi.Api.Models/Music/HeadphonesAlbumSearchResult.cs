#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: HeadphonesAlbumSearchResult.cs
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
namespace Ombi.Api.Models.Music
{
    public class HeadphonesAlbumSearchResult
    {
        public string rgid { get; set; }
        public string albumurl { get; set; }
        public string tracks { get; set; }
        public string date { get; set; }
        public string id { get; set; } // Artist ID
        public string rgtype { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string country { get; set; }
        public string albumid { get; set; } // AlbumId
        public int score { get; set; }
        public string uniquename { get; set; }
        public string formats { get; set; }
    }
}