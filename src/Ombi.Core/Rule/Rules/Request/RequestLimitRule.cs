#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: RequestLimitRule.cs
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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Request
{
    public class RequestLimitRule : BaseRequestRule, IRules<BaseRequest>
    {
        public RequestLimitRule(IRepository<RequestLog> rl, OmbiUserManager um)
        {
            _requestLog = rl;
            _userManager = um;
        }

        private readonly IRepository<RequestLog> _requestLog;
        private readonly OmbiUserManager _userManager;

        public async Task<RuleResult> Execute(BaseRequest obj)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == obj.RequestedUserId);

            var movieLimit = user.MovieRequestLimit;
            var episodeLimit = user.EpisodeRequestLimit;
            var musicLimit = user.MusicRequestLimit;

            var requestLog = _requestLog.GetAll().Where(x => x.UserId == obj.RequestedUserId);
            if (obj.RequestType == RequestType.Movie)
            {
                if (movieLimit <= 0)
                    return Success();
                
                var movieLogs = requestLog.Where(x => x.RequestType == RequestType.Movie);
                
                // Count how many requests in the past 7 days
                var count = await movieLogs.CountAsync(x => x.RequestDate >= DateTime.UtcNow.AddDays(-7));
                count += 1; // Since we are including this request
                if (count > movieLimit)
                {
                    return Fail("You have exceeded your Movie request quota!");
                }
            }
            else if (obj.RequestType == RequestType.TvShow)
            {
                if (episodeLimit <= 0)
                    return Success();
                
                var child = (ChildRequests) obj;
                var requestCount = 0;
                // Get the count of requests to be made
                foreach (var s in child.SeasonRequests)
                {
                    requestCount += s.Episodes.Count;
                }

                var tvLogs = requestLog.Where(x => x.RequestType == RequestType.TvShow);

                // Count how many requests in the past 7 days
                var tv = tvLogs.Where(x => x.RequestDate >= DateTime.UtcNow.AddDays(-7));

                // Needed, due to a bug which would cause all episode counts to be 0
                var zeroEpisodeCount = await tv.Where(x => x.EpisodeCount == 0).Select(x => x.EpisodeCount).CountAsync();

                var episodeCount = await tv.Where(x => x.EpisodeCount != 0).Select(x => x.EpisodeCount).SumAsync();

                var count = requestCount + episodeCount + zeroEpisodeCount; // Add the amount of requests in
                if (count > episodeLimit)
                {
                    return Fail("You have exceeded your Episode request quota!");
                }
            } else if (obj.RequestType == RequestType.Album)
            {
                if (musicLimit <= 0)
                    return Success();

                var albumLogs = requestLog.Where(x => x.RequestType == RequestType.Album);

                // Count how many requests in the past 7 days
                var count = await albumLogs.CountAsync(x => x.RequestDate >= DateTime.UtcNow.AddDays(-7));
                count += 1; // Since we are including this request
                if (count > musicLimit)
                {
                    return Fail("You have exceeded your Album request quota!");
                }
            }
                return Success();
        }
    }
}
