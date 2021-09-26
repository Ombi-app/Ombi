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
using Ombi.Core.Services;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Request
{
    public class RequestLimitRule : BaseRequestRule, IRules<BaseRequest>
    {
        public RequestLimitRule(IRequestLimitService requestLimitService)
        {
            _requestLimitService = requestLimitService;
        }

        private readonly IRequestLimitService _requestLimitService;

        public async Task<RuleResult> Execute(BaseRequest obj)
        {
            if (obj.RequestType == RequestType.Movie)
            {
                var remainingLimitsModel = await _requestLimitService.GetRemainingMovieRequests();
                if (!remainingLimitsModel.HasLimit)
                {
                    return Success();
                }

                if (remainingLimitsModel.Remaining < 1)
                {
                    return Fail("You have exceeded your Movie request quota!");
                }
            }
            if (obj.RequestType == RequestType.TvShow)
            {
                var remainingLimitsModel = await _requestLimitService.GetRemainingTvRequests();
                if (!remainingLimitsModel.HasLimit)
                {
                    return Success();
                }

                var child = (ChildRequests)obj;
                var requestCount = 0;
                // Get the count of requests to be made
                foreach (var s in child.SeasonRequests)
                {
                    requestCount += s.Episodes.Count;
                }

                if ((remainingLimitsModel.Remaining - requestCount) < 0)
                {
                    return Fail("You have exceeded your Episode request quota!");
                }
            }
            if (obj.RequestType == RequestType.Album)
            {
                var remainingLimitsModel = await _requestLimitService.GetRemainingMusicRequests();
                if (!remainingLimitsModel.HasLimit)
                {
                    return Success();
                }

                if (remainingLimitsModel.Remaining < 1)
                {
                    return Fail("You have exceeded your Album request quota!");
                }
            }
            return Success();
        }
    }
}
