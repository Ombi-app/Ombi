#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: ApiSettingsMetadataModule.cs
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
using Nancy.Metadata.Modules;
using Nancy.Swagger;

using PlexRequests.Core.SettingModels;

namespace PlexRequests.UI.Modules
{
    public class ApiSettingsMetadataModule: MetadataModule<SwaggerRouteData>
    {
        public ApiSettingsMetadataModule()
        {
            Describe["GetAuthSettings"] = description => description.AsSwagger(with =>
            {
                with.ResourcePath("/settings/authentication");
                with.Summary("Gets the authentication settings saved in the application");
                with.Model<ApiModel<AuthenticationSettings>>();
                with.Notes("Gets the authentication settings saved in the application");

                with.QueryParam<string>("apikey", "The Api Key found in the settings", true);
            });

            Describe["PostAuthSettings"] = description => description.AsSwagger(with =>
            {
                with.ResourcePath("/settings/authentication");
                with.Summary("Saves the authentication settings saved in the application");
                with.Model<ApiModel<bool>>();
                with.QueryParam<string>("apikey", "The Api Key found in the settings", true);
                with.BodyParam<AuthenticationSettings>("Authentication settings", true);
                with.Notes("Saves the authentication settings saved in the application");
            });

            Describe["GetPlexSettings"] = description => description.AsSwagger(with =>
            {
                with.ResourcePath("/settings/plex");
                with.Summary("Gets the Plex settings saved in the application");
                with.Model<ApiModel<PlexSettings>>();
                with.Notes("Gets the Plex settings saved in the application");
                with.QueryParam<string>("apikey", "The Api Key found in the settings", true);
            });

            Describe["PostPlexSettings"] = description => description.AsSwagger(with =>
            {
                with.ResourcePath("/settings/plex");
                with.Summary("Saves the Plex settings saved in the application");
                with.Model<ApiModel<bool>>();
                with.QueryParam<string>("apikey", "The Api Key found in the settings", true);
                with.BodyParam<PlexSettings>("Plex settings", true);
                with.Notes("Saves the Plex settings saved in the application");
            });


            Describe["GetCouchPotatoSettings"] = description => description.AsSwagger(with =>
            {
                with.ResourcePath("/settings/couchpotato");
                with.Summary("Gets the CouchPotato settings saved in the application");
                with.Model<ApiModel<CouchPotatoSettings>>();
                with.Notes("Gets the CouchPotato settings saved in the application");
                with.QueryParam<string>("apikey", "The Api Key found in the settings", true);
            });

            Describe["PostCouchPotatoSettings"] = description => description.AsSwagger(with =>
            {
                with.ResourcePath("/settings/couchpotato");
                with.Summary("Saves the CouchPotato settings saved in the application");
                with.Model<ApiModel<bool>>();
                with.QueryParam<string>("apikey", "The Api Key found in the settings", true);
                with.BodyParam<CouchPotatoSettings>("CouchPotato settings", true);
                with.Notes("Saves the CouchPotato settings saved in the application");
            });

            Describe["GetSonarrSettings"] = description => description.AsSwagger(with =>
            {
                with.ResourcePath("/settings/sonarr");
                with.Summary("Gets the sonarr settings saved in the application");
                with.Model<ApiModel<SonarrSettings>>();
                with.Notes("Gets the sonarr settings saved in the application");
                with.QueryParam<string>("apikey", "The Api Key found in the settings", true);
            });

            Describe["PostSonarrSettings"] = description => description.AsSwagger(with =>
            {
                with.ResourcePath("/settings/sonarr");
                with.Summary("Saves the sonarr settings saved in the application");
                with.Model<ApiModel<bool>>();
                with.QueryParam<string>("apikey", "The Api Key found in the settings", true);
                with.BodyParam<SonarrSettings>("sonarr settings", true);
                with.Notes("Saves the sonarr settings saved in the application");
            });

            Describe["GetSickRageSettings"] = description => description.AsSwagger(with =>
            {
                with.ResourcePath("/settings/sickrage");
                with.Summary("Gets the SickRage settings saved in the application");
                with.Model<ApiModel<SickRageSettings>>();
                with.Notes("Gets the SickRage settings saved in the application");
                with.QueryParam<string>("apikey", "The Api Key found in the settings", true);
            });

            Describe["PostSickRageSettings"] = description => description.AsSwagger(with =>
            {
                with.ResourcePath("/settings/sickrage");
                with.Summary("Saves the SickRage settings saved in the application");
                with.Model<ApiModel<bool>>();
                with.QueryParam<string>("apikey", "The Api Key found in the settings", true);
                with.BodyParam<SickRageSettings>("SickRage settings", true);
                with.Notes("Saves the sickrage settings saved in the application");
            });

            Describe["GetHeadphonesSettings"] = description => description.AsSwagger(with =>
            {
                with.ResourcePath("/settings/headphones");
                with.Summary("Gets the headphones settings saved in the application");
                with.Model<ApiModel<HeadphonesSettings>>();
                with.Notes("Gets the headphones settings saved in the application");
                with.QueryParam<string>("apikey", "The Api Key found in the settings", true);
            });

            Describe["PostHeadphonesSettings"] = description => description.AsSwagger(with =>
            {
                with.ResourcePath("/settings/sickrage");
                with.Summary("Saves the headphones settings saved in the application");
                with.Model<ApiModel<bool>>();
                with.QueryParam<string>("apikey", "The Api Key found in the settings", true);
                with.BodyParam<HeadphonesSettings>("headphones settings", true);
                with.Notes("Saves the headphones settings saved in the application");
            });
        }
    }
}