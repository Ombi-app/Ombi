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
using Nancy;
using Nancy.ModelBinding;

using PlexRequests.Core;
using PlexRequests.Core.SettingModels;

namespace PlexRequests.UI.Modules
{
    public class ApiSettingsModule : BaseApiModule
    {
        public ApiSettingsModule(ISettingsService<PlexRequestSettings> pr, ISettingsService<AuthenticationSettings> auth) : base("api", pr)
        {
            Get["GetAuthSettings","/settings/authentication"] = x => GetAuthSettings();
            Post["PostAuthSettings","/settings/authentication"] = x => PostAuthSettings();

            SettingsService = pr;
            AuthSettings = auth;
        }
        
        private ISettingsService<PlexRequestSettings> SettingsService { get; }
        private ISettingsService<AuthenticationSettings> AuthSettings { get; }

        public Response GetAuthSettings()
        {
            var model = new ApiModel<AuthenticationSettings>();
            var settings = AuthSettings.GetSettings();
            model.Data = settings;
            return ReturnReponse(model);
        }

        public Response PostAuthSettings()
        {
            var newSettings = this.BindAndValidate<AuthenticationSettings>();
            if (!ModelValidationResult.IsValid)
            {
                return ReturnValidationReponse(ModelValidationResult);
            }

            var model = new ApiModel<bool>();
            var settings = AuthSettings.SaveSettings(newSettings);
            if (settings)
            {
                model.Data = true;
                return ReturnReponse(model);
            }

            model.Error = true;
            model.ErrorMessage = "Could not update the settings";
            return ReturnReponse(model);
        }   

    }
}