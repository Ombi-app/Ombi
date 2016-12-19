#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: ApiUserMetadataModule.cs
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
using Ombi.UI.Models;
using Ombi.UI.Models.UserManagement;

namespace Ombi.UI.Modules
{
    public class ApiUserMetadataModule: MetadataModule<SwaggerRouteData>
    {
        public ApiUserMetadataModule()
        {
            Describe["GetApiKey"] = description => description.AsSwagger(with =>
            {
                with.ResourcePath("/apikey");
                with.Summary("Gets the Api Key for Plex Requests");
                with.Model<ApiModel<string>>();
                with.QueryParam<string>("username", required:true );
                with.QueryParam<string>("password", required: true );
                with.Notes("Get's the current api key for the application");
            });

            Describe["PutCredentials"] = description => description.AsSwagger(with =>
            {
                with.ResourcePath("/credentials/{username}");
                with.Summary("Sets a new password for the user");
                with.Model<ApiModel<string>>();
                with.PathParam<int>("username", required:true);
                with.QueryParam<string>("apikey", "The Api Key found in the settings", true);
                with.BodyParam<UserUpdateViewModel>("User update view model", true);
                with.Notes("Sets a new password for the user");
            });

        }
    }
}