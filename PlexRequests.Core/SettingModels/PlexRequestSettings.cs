#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: PlexRequestSettings.cs
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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PlexRequests.Core.SettingModels
{
    public class PlexRequestSettings : Settings
    {
        public PlexRequestSettings()
        {
            TvWeeklyRequestLimit = 0;
            MovieWeeklyRequestLimit = 0;
            AlbumWeeklyRequestLimit = 0;
        }

        public int Port { get; set; }
        public string BaseUrl { get; set; }
        public bool SearchForMovies { get; set; }
        public bool SearchForTvShows { get; set; }
        public bool SearchForMusic { get; set; }
        public bool RequireMovieApproval { get; set; }
        public bool RequireTvShowApproval { get; set; }
        public bool RequireMusicApproval { get; set; }
        public bool UsersCanViewOnlyOwnRequests { get; set; }
        public bool UsersCanViewOnlyOwnIssues { get; set; }
        public int MovieWeeklyRequestLimit { get; set; }
        public int TvWeeklyRequestLimit { get; set; }
        public int AlbumWeeklyRequestLimit { get; set; }
        public string NoApprovalUsers { get; set; }
        public bool CollectAnalyticData { get; set; }
        public bool IgnoreNotifyForAutoApprovedRequests { get; set; }

        /// <summary>
        /// The CSS name of the theme we want
        /// </summary>
        public string ThemeName { get; set; }

        public string ApiKey { get; set; }

        [JsonIgnore]
        public List<string> ApprovalWhiteList
        {
            get
            {
                var users = new List<string>();
                if (string.IsNullOrEmpty(NoApprovalUsers))
                {
                    return users;
                }

                var splitUsers = NoApprovalUsers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var user in splitUsers)
                {
                    if (!string.IsNullOrWhiteSpace(user))
                        users.Add(user.Trim());
                }
                return users;
            }
        }
    }
}
