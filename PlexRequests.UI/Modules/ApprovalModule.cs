#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: ApprovalModule.cs
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

using Nancy;
using Nancy.Security;

using NLog;

using PlexRequests.Store;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class ApprovalModule : BaseModule
    {

        public ApprovalModule(IRepository<RequestedModel> service) : base("approval")
        {
            this.RequiresAuthentication();

            Service = service;

            Post["/approve"] = parameters => Approve((int)Request.Form.requestid);
            Post["/approveall"] = x => ApproveAll();
        }

        private IRepository<RequestedModel> Service { get; set; }

        private static Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Approves the specified request identifier.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <returns></returns>
        private Response Approve(int requestId)
        {
            // Get the request from the DB
            var request = Service.Get(requestId);

            if (request == null)
            {
                Log.Warn("Tried approving a request, but the request did not exist in the database, requestId = {0}", requestId);
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "There are no requests to approve. Please refresh." });
            }

            // Approve it
            request.Approved = true;

            // Update the record
            var result = Service.Update(request);

            return Response.AsJson(result
                ? new JsonResponseModel { Result = true }
                : new JsonResponseModel { Result = false, Message = "We could not approve this request. Please try again or check the logs." });
        }

        /// <summary>
        /// Approves all.
        /// </summary>
        /// <returns></returns>
        private Response ApproveAll()
        {
            var requests = Service.GetAll();
            var requestedModels = requests as RequestedModel[] ?? requests.ToArray();
            if (!requestedModels.Any())
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "There are no requests to approve. Please refresh." });
            }

            var updatedRequests = new List<RequestedModel>();
            foreach (var r in requestedModels)
            {
                r.Approved = true;
                updatedRequests.Add(r);
            }
            try
            {

                var result = Service.UpdateAll(updatedRequests); return Response.AsJson(result
                 ? new JsonResponseModel { Result = true }
                 : new JsonResponseModel { Result = false, Message = "We could not approve all of the requests. Please try again or check the logs." });

            }
            catch (Exception e)
            {
                Log.Fatal(e);
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Something bad happened, please check the logs!" });
            }

        }
    }
}