#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: ApiRequestModule.cs
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

using Nancy;
using Nancy.ModelBinding;

using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Store;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class ApiRequestModule : BaseApiModule
    {
        public ApiRequestModule(IRequestService service, ISettingsService<PlexRequestSettings> pr) : base("api", pr)
        {
            Get["GetRequests","/requests"] = x => GetRequests();
            Get["GetRequest","/requests/{id}"] = x => GetSingleRequests(x);
            Post["PostRequests", "/requests"] = x => CreateRequest();
            Put["PutRequests", "/requests"] = x => UpdateRequest();
            Delete["DeleteRequests", "/requests/{id}"] = x => DeleteRequest(x);

            
            RequestService = service;
            SettingsService = pr;
        }

        private IRequestService RequestService { get; }
        private ISettingsService<PlexRequestSettings> SettingsService { get; }

        public Response GetRequests()
        {
            var apiModel = new ApiModel<List<RequestedModel>> { Data = new List<RequestedModel>() };

            var requests = RequestService.GetAll();
            apiModel.Data.AddRange(requests);

            return ReturnReponse(apiModel);
        }

        public Response GetSingleRequests(dynamic x)
        {
            var id = (int)x.id;
            var apiModel = new ApiModel<List<RequestedModel>> { Data = new List<RequestedModel>() };

            var requests = RequestService.Get(id);
            if (string.IsNullOrEmpty(requests.Title))
            {
                apiModel.Error = true;
                apiModel.ErrorMessage = "Request does not exist";
                return ReturnReponse(apiModel);
            }
            apiModel.Data.Add(requests);

            return ReturnReponse(apiModel);
        }

        public Response CreateRequest()
        {
            var request = this.BindAndValidate<RequestedModel>();

            if (!ModelValidationResult.IsValid)
            {
                return ReturnValidationReponse(ModelValidationResult);
            }


            var apiModel = new ApiModel<bool>();
            var result = RequestService.AddRequest(request);

            if (result == -1)
            {
                apiModel.Error = true;
                apiModel.ErrorMessage = "Could not insert the new request into the database. Internal error.";
                return ReturnReponse(apiModel);
            }

            apiModel.Data = true;

            return ReturnReponse(apiModel);
        }

        public Response UpdateRequest()
        {
            var request = this.BindAndValidate<RequestedModel>();

            if (!ModelValidationResult.IsValid)
            {
                return ReturnValidationReponse(ModelValidationResult);
            }


            var apiModel = new ApiModel<bool>();
            var result = RequestService.UpdateRequest(request);

            if (!result)
            {
                apiModel.Error = true;
                apiModel.ErrorMessage = "Could not update the request into the database. Internal error.";
                return ReturnReponse(apiModel);
            }

            apiModel.Data = true;

            return ReturnReponse(apiModel);
        }

        public Response DeleteRequest(dynamic x)
        {
            var id = (int)x.id;
            var apiModel = new ApiModel<bool>();

            try
            {
                var exisitingRequest = RequestService.Get(id);
                if (string.IsNullOrEmpty(exisitingRequest.Title))
                {
                    apiModel.Error = true;
                    apiModel.ErrorMessage = $"The request id {id} does not exist";
                    return ReturnReponse(apiModel);
                }
                RequestService.DeleteRequest(exisitingRequest);
                apiModel.Data = true;

                return ReturnReponse(apiModel);
            }
            catch (Exception)
            {
                apiModel.Error = true;
                apiModel.ErrorMessage = "Could not delete the request from the database. Internal error.";
                return ReturnReponse(apiModel);
            }
        }


    }
}