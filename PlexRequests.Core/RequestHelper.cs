#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: RequestHelper.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using PlexRequests.Core.SettingModels;
using PlexRequests.Store;

namespace PlexRequests.Core
{
    public static class RequestHelper
    {
        public static bool ShouldAutoApprove(this RequestType requestType, PlexRequestSettings prSettings, bool isAdmin, string username)
        {
            // if the user is an admin or they are whitelisted, they go ahead and allow auto-approval
            if (isAdmin || prSettings.ApprovalWhiteList.Any(x => x.Equals(username, StringComparison.OrdinalIgnoreCase))) return true;

            // check by request type if the category requires approval or not
            switch (requestType)
            {
                case RequestType.Movie:
                    return !prSettings.RequireMovieApproval;
                case RequestType.TvShow:
                    return !prSettings.RequireTvShowApproval;
                case RequestType.Album:
                    return !prSettings.RequireMusicApproval;
                default:
                    return false;
            }
        }

        public static bool ShouldAutoApprove(this RequestType requestType, PlexRequestSettings prSettings, bool isAdmin, List<string> username)
        {
            // if the user is an admin or they are whitelisted, they go ahead and allow auto-approval
            if (isAdmin) return true;


            if(prSettings.ApprovalWhiteList.Intersect(username).Any())
            {
                return true;
            }

            // check by request type if the category requires approval or not
            switch (requestType)
            {
                case RequestType.Movie:
                    return !prSettings.RequireMovieApproval;
                case RequestType.TvShow:
                    return !prSettings.RequireTvShowApproval;
                case RequestType.Album:
                    return !prSettings.RequireMusicApproval;
                default:
                    return false;
            }
        }
    }
}