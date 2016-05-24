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

        }
    }
}