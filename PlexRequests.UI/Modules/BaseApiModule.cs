#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: BaseApiModule.cs
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
using System.Linq;

using Nancy;
using Nancy.Validation;

using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Store;

namespace PlexRequests.UI.Modules
{
    public abstract class BaseApiModule : BaseModule
    {
        protected BaseApiModule(ISettingsService<PlexRequestSettings> s) : base(s)
        {
            Settings = s;
            Before += (ctx) => CheckAuth();
        }

        protected BaseApiModule(string modulePath, ISettingsService<PlexRequestSettings> s) : base(modulePath, s)
        {
            Settings = s;
            Before += (ctx) => CheckAuth();
        }

        private ISettingsService<PlexRequestSettings> Settings { get; }

        protected Response ReturnReponse(object result)
        {
            var queryString = (DynamicDictionary)Context.Request.Query;
            dynamic value;
            if (queryString.TryGetValue("xml", out value))
            {
                if ((bool)value)
                {
                    return Response.AsXml(result);
                }
            }
            return Response.AsJson(result);
        }

        protected Response ReturnValidationReponse(ModelValidationResult result)
        {
            var errors = result.Errors;
            var model = new ApiModel<List<string>>
            {
                Error = true,
                ErrorMessage = "Please view the error messages inside the data node",
                Data = new List<string>()
            };

            foreach (var error in errors)
            {
                model.Data.AddRange(error.Value.Select(x => x.ErrorMessage));
            }

            return ReturnReponse(model);
        }

        private Response CheckAuth()
        {
            var settings = Settings.GetSettings();
            var apiModel = new ApiModel<List<RequestedModel>> { Data = new List<RequestedModel>() };
            if (!Authenticated(settings))
            {
                apiModel.Error = true;
                apiModel.ErrorMessage = "ApiKey is invalid or not present, Please use 'apikey' in the querystring.";
                return ReturnReponse(apiModel);
            }

            return null;
        }

        private bool Authenticated(PlexRequestSettings settings)
        {
            var query = (DynamicDictionary)Context.Request.Query;
            dynamic key;
            if (!query.TryGetValue("apikey", out key))
            {
                return false;
            }
            if ((string)key == settings.ApiKey)
            {
                return true;
            }
            return false;

        }
    }
}