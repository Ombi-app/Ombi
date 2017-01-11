#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: SearchTvShowViewModel.cs
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

namespace Ombi.UI.Models
{
    public class SearchTvShowViewModel : SearchViewModel
    {
        public SearchTvShowViewModel()
        {
            Episodes = new List<Store.EpisodesModel>();
        }
        public int Id { get; set; }
        public string SeriesName { get; set; }
        public List<string> Aliases { get; set; }
        public string Banner { get; set; }
        public int SeriesId { get; set; }
        public string Status { get; set; }
        public string FirstAired { get; set; }
        public string Network { get; set; }
        public string NetworkId { get; set; }
        public string Runtime { get; set; }
        public List<string> Genre { get; set; }
        public string Overview { get; set; }
        public int LastUpdated { get; set; }
        public string AirsDayOfWeek { get; set; }
        public string AirsTime { get; set; }
        public string Rating { get; set; }
        public string ImdbId { get; set; }
        public int SiteRating { get; set; }
        public List<Store.EpisodesModel> Episodes { get; set; }
        public bool TvFullyAvailable { get; set; }
        public bool DisableTvRequestsByEpisode { get; set; }
        public bool DisableTvRequestsBySeason { get; set; }
        public bool EnableTvRequestsForOnlySeries { get; set; }
    }
}