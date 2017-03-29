#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: AppveyorApi.cs
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
using NLog;
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Appveyor;
using Ombi.Helpers;
using RestSharp;

namespace Ombi.Api
{
    public class AppveyorApi : IAppveyorApi
    {
        private const string AppveyorApiUrl = "https://ci.appveyor.com/api";

        private const string Key =
            "48Ku58C0794nBrXra8IxWav+dc6NqgkRw+PZB3/bQwbt/D0IrnJQkgtjzo0bd6nkooLMKsC8M+Ab7jyBO+ROjY14VRuxffpDopX9r0iG/fjBl6mZVvqkm+VTDNstDtzp";


        public AppveyorApi()
        {
            Api = new ApiRequest();
        }
        private ApiRequest Api { get; set; }
        private static Logger Log = LogManager.GetCurrentClassLogger();

        //https://ci.appveyor.com/api/projects/tidusjar/requestplex/history?recordsNumber=10&branch=eap
        public AppveyorProjects GetProjectHistory(string branchName, int records = 10)
        {
            var request = new RestRequest
            {
                Resource = "projects/tidusjar/requestplex/history?recordsNumber={records}&branch={branch}",
                Method = Method.GET
            };

            request.AddUrlSegment("records", records.ToString());
            request.AddUrlSegment("branch", branchName);
            AddHeaders(request);

            var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling GetProjectHistory for Appveyor, Retrying {0}", timespan), new[] {
                TimeSpan.FromSeconds (1),
            });

            var obj = policy.Execute(() => Api.ExecuteJson<AppveyorProjects>(request, new Uri(AppveyorApiUrl)));

            return obj;
        }

        private void AddHeaders(IRestRequest request)
        {
            request.AddHeader("Authorization", $"Bearer {Key}");
            request.AddHeader("Content-Type", "application/json");
        }
    }
}