#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: SickRageShowInformation.cs
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

using System.Collections.Generic;

namespace PlexRequests.Api.Models.SickRage
{
    public class Cache
    {
        public int banner { get; set; }
        public int poster { get; set; }
    }

    public class QualityDetails
    {
        public List<object> archive { get; set; }
        public List<string> initial { get; set; }
    }

    public class SeasonList
    {
    }

    public class Data
    {
        public int air_by_date { get; set; }
        public string airs { get; set; }
        public int anime { get; set; }
        public int archive_firstmatch { get; set; }
        public Cache cache { get; set; }
        public int dvdorder { get; set; }
        public int flatten_folders { get; set; }
        public List<string> genre { get; set; }
        public string imdbid { get; set; }
        public int indexerid { get; set; }
        public string language { get; set; }
        public string location { get; set; }
        public string network { get; set; }
        public string next_ep_airdate { get; set; }
        public int paused { get; set; }
        public string quality { get; set; }
        public QualityDetails quality_details { get; set; }
        public List<object> rls_ignore_words { get; set; }
        public List<object> rls_require_words { get; set; }
        public int scene { get; set; }
        public SeasonList season_list { get; set; }
        public string show_name { get; set; }
        public int sports { get; set; }
        public string status { get; set; }
        public int subtitles { get; set; }
        public int tvdbid { get; set; }
    }

    public class SickRageShowInformation : SickRageBase<Data>
    {
    }

}