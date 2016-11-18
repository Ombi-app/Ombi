#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: ApiModule.cs
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
using PlexRequests.UI.Helpers;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class ApiUserModule : BaseApiModule
    {
        public ApiUserModule(ISettingsService<PlexRequestSettings> pr, ICustomUserMapper m, ISecurityExtensions security) : base("api", pr, security)
        {

            Put["PutCredentials", "/credentials/{username}"] = x => ChangePassword(x);

            Get["GetApiKey", "/apikey"] = x => GetApiKey();

            SettingsService = pr;
            UserMapper = m;
        }
        
        private ISettingsService<PlexRequestSettings> SettingsService { get; }
        private ICustomUserMapper UserMapper { get; }

        public Response ChangePassword(dynamic x)
        {
            var username = (string)x.username;
            var userModel = this.BindAndValidate<UserUpdateViewModel>();

            if (!ModelValidationResult.IsValid)
            {
                return ReturnValidationReponse(ModelValidationResult);
            }

            var valid = UserMapper.ValidateUser(username, userModel.CurrentPassword);
            if (valid == null)
            {
                var errorModel = new ApiModel<string> { Error = true, ErrorMessage = "Incorrect username or password" };
                return ReturnReponse(errorModel);
            }
            var result = UserMapper.UpdatePassword(username, userModel.CurrentPassword, userModel.NewPassword);

            if (!result)
            {
                var errorModel = new ApiModel<string> { Error = true, ErrorMessage = "Could not update the password. " };
                return ReturnReponse(errorModel);
            }


            var model = new ApiModel<string> { Data = "Successfully updated the password"};
            return ReturnReponse(model);
        }
        public Response GetApiKey()
        {
            var user = Request.Query["username"];
            var password = Request.Query["password"];
            var result = UserMapper.ValidateUser(user, password);
            var model = new ApiModel<string>();
            if (result == null)
            {
                model.Error = true;
                model.ErrorMessage = "Incorrect username or password";
                return ReturnReponse(model);
            }

            var settings = SettingsService.GetSettings();
            model.Data = settings.ApiKey;

            return ReturnReponse(model);
        }   

    }
}