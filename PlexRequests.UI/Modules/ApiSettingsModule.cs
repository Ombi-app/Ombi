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
using System.Web.UI.WebControls;

using Nancy;
using Nancy.Extensions;
using Nancy.ModelBinding;
using Nancy.Validation;

using Newtonsoft.Json;

using PlexRequests.Core;
using PlexRequests.Core.SettingModels;

namespace PlexRequests.UI.Modules
{
    public class ApiSettingsModule : BaseApiModule
    {
        public ApiSettingsModule(ISettingsService<PlexRequestSettings> pr, ISettingsService<AuthenticationSettings> auth,
            ISettingsService<PlexSettings> plexSettings, ISettingsService<CouchPotatoSettings> cp,
            ISettingsService<SonarrSettings> sonarr, ISettingsService<SickRageSettings> sr, ISettingsService<HeadphonesSettings> hp) : base("api", pr)
        {
            Get["GetAuthSettings", "/settings/authentication"] = x => GetAuthSettings();
            Post["PostAuthSettings", "/settings/authentication"] = x => PostAuthSettings();

            Get["GetPlexRequestSettings", "/settings/plexrequest"] = x => GetPrSettings();
            Post["PostPlexRequestSettings", "/settings/plexrequest"] = x => PostPrSettings();

            Get["GetPlexSettings", "/settings/plex"] = x => GetPlexSettings();
            Post["PostPlexSettings", "/settings/plex"] = x => PostPlexSettings();

            Get["GetCouchPotatoSettings", "/settings/couchpotato"] = x => GetCpSettings();
            Post["PostCouchPotatoSettings", "/settings/couchpotato"] = x => PostCpSettings();

            Get["GetSonarrSettings", "/settings/sonarr"] = x => GetSonarrSettings();
            Post["PostSonarrSettings", "/settings/sonarr"] = x => PostSonarrSettings();

            Get["GetSickRageSettings", "/settings/sickrage"] = x => GetSickRageSettings();
            Post["PostSickRageSettings", "/settings/sickrage"] = x => PostSickRageSettings();

            Get["GetHeadphonesSettings", "/settings/headphones"] = x => GetHeadphonesSettings();
            Post["PostHeadphonesSettings", "/settings/headphones"] = x => PostHeadphonesSettings();

            SettingsService = pr;
            AuthSettings = auth;
            PlexSettings = plexSettings;
            CpSettings = cp;
            SonarrSettings = sonarr;
            SickRageSettings = sr;
            HeadphonesSettings = hp;
        }

        private ISettingsService<PlexRequestSettings> SettingsService { get; }
        private ISettingsService<AuthenticationSettings> AuthSettings { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<CouchPotatoSettings> CpSettings { get; }
        private ISettingsService<SonarrSettings> SonarrSettings { get; }
        private ISettingsService<SickRageSettings> SickRageSettings { get; }
        private ISettingsService<HeadphonesSettings> HeadphonesSettings { get; }

        private Response GetPrSettings()
        {
            var model = new ApiModel<PlexRequestSettings>();
            try
            {
                var settings = SettingsService.GetSettings();
                model.Data = settings;
                return ReturnReponse(model);
            }
            catch (Exception e)
            {
                model.ErrorMessage = e.Message;
                model.Error = true;
                return ReturnReponse(model);
            }
        }

        private Response PostPrSettings()
        {
            var newSettings = JsonConvert.DeserializeObject<PlexRequestSettings>(Request.Body.AsString());
            var result = this.Validate(newSettings);
            if (!result.IsValid)
            {
                return ReturnValidationReponse(result);
            }

            var model = new ApiModel<bool>();
            var settings = SettingsService.SaveSettings(newSettings);
            if (settings)
            {
                model.Data = true;
                return ReturnReponse(model);
            }

            model.Error = true;
            model.ErrorMessage = "Could not update the settings";
            return ReturnReponse(model);
        }

        private Response GetAuthSettings()
        {
            var model = new ApiModel<AuthenticationSettings>();
            try
            {
                var settings = AuthSettings.GetSettings();
                model.Data = settings;
                return ReturnReponse(model);
            }
            catch (Exception e)
            {
                model.ErrorMessage = e.Message;
                model.Error = true;
                return ReturnReponse(model);
            }
        }

        private Response PostAuthSettings()
        {
            var newSettings = JsonConvert.DeserializeObject<AuthenticationSettings>(Request.Body.AsString());
            var result = this.Validate(newSettings);
            if (!result.IsValid)
            {
                return ReturnValidationReponse(result);
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

        private Response GetPlexSettings()
        {
            var model = new ApiModel<PlexSettings>();
            try
            {
                var settings = PlexSettings.GetSettings();
                model.Data = settings;
                return ReturnReponse(model);
            }
            catch (Exception e)
            {
                model.ErrorMessage = e.Message;
                model.Error = true;
                return ReturnReponse(model);
            }
        }

        private Response PostPlexSettings()
        {
            var newSettings = JsonConvert.DeserializeObject<PlexSettings>(Request.Body.AsString());
            var result = this.Validate(newSettings);
            if (!result.IsValid)
            {
                return ReturnValidationReponse(result);
            }

            var model = new ApiModel<bool>();
            var settings = PlexSettings.SaveSettings(newSettings);
            if (settings)
            {
                model.Data = true;
                return ReturnReponse(model);
            }

            model.Error = true;
            model.ErrorMessage = "Could not update the settings";
            return ReturnReponse(model);
        }

        private Response GetCpSettings()
        {
            var model = new ApiModel<CouchPotatoSettings>();
            try
            {
                var settings = CpSettings.GetSettings();
                model.Data = settings;
                return ReturnReponse(model);
            }
            catch (Exception e)
            {
                model.ErrorMessage = e.Message;
                model.Error = true;
                return ReturnReponse(model);
            }
        }

        private Response PostCpSettings()
        {
            var newSettings = JsonConvert.DeserializeObject<CouchPotatoSettings>(Request.Body.AsString());
            var result = this.Validate(newSettings);
            if (!result.IsValid)
            {
                return ReturnValidationReponse(result);
            }

            var model = new ApiModel<bool>();
            var settings = CpSettings.SaveSettings(newSettings);
            if (settings)
            {
                model.Data = true;
                return ReturnReponse(model);
            }

            model.Error = true;
            model.ErrorMessage = "Could not update the settings";
            return ReturnReponse(model);
        }

        private Response GetSonarrSettings()
        {
            var model = new ApiModel<SonarrSettings>();
            try
            {
                var settings = SonarrSettings.GetSettings();
                model.Data = settings;
                return ReturnReponse(model);
            }
            catch (Exception e)
            {
                model.ErrorMessage = e.Message;
                model.Error = true;
                return ReturnReponse(model);
            }
        }

        private Response PostSonarrSettings()
        {
            var newSettings = JsonConvert.DeserializeObject<SonarrSettings>(Request.Body.AsString());
            var result = this.Validate(newSettings);
            if (!result.IsValid)
            {
                return ReturnValidationReponse(result);
            }

            var model = new ApiModel<bool>();
            var settings = SonarrSettings.SaveSettings(newSettings);
            if (settings)
            {
                model.Data = true;
                return ReturnReponse(model);
            }

            model.Error = true;
            model.ErrorMessage = "Could not update the settings";
            return ReturnReponse(model);
        }

        private Response GetSickRageSettings()
        {
            var model = new ApiModel<SickRageSettings>();
            try
            {
                var settings = SickRageSettings.GetSettings();
                model.Data = settings;
                return ReturnReponse(model);
            }
            catch (Exception e)
            {
                model.ErrorMessage = e.Message;
                model.Error = true;
                return ReturnReponse(model);
            }
        }

        private Response PostSickRageSettings()
        {
            var newSettings = JsonConvert.DeserializeObject<SickRageSettings>(Request.Body.AsString());
            var result = this.Validate(newSettings);
            if (!result.IsValid)
            {
                return ReturnValidationReponse(result);
            }

            var model = new ApiModel<bool>();
            var settings = SickRageSettings.SaveSettings(newSettings);
            if (settings)
            {
                model.Data = true;
                return ReturnReponse(model);
            }

            model.Error = true;
            model.ErrorMessage = "Could not update the settings";
            return ReturnReponse(model);
        }
        private Response GetHeadphonesSettings()
        {
            var model = new ApiModel<HeadphonesSettings>();
            try
            {
                var settings = HeadphonesSettings.GetSettings();
                model.Data = settings;
                return ReturnReponse(model);
            }
            catch (Exception e)
            {
                model.ErrorMessage = e.Message;
                model.Error = true;
                return ReturnReponse(model);
            }
        }

        private Response PostHeadphonesSettings()
        {
            var newSettings = JsonConvert.DeserializeObject<HeadphonesSettings>(Request.Body.AsString());
            var result = this.Validate(newSettings);
            if (!result.IsValid)
            {
                return ReturnValidationReponse(result);
            }

            var model = new ApiModel<bool>();
            var settings = HeadphonesSettings.SaveSettings(newSettings);
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