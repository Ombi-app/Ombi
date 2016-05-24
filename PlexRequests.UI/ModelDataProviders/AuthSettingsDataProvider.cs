#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: AuthSettingsDataProvider.cs
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
using Nancy.Swagger;
using Nancy.Swagger.Services;

using PlexRequests.Core.SettingModels;

namespace PlexRequests.UI.ModelDataProviders
{
    public class AuthSettingsDataProvider : ISwaggerModelDataProvider
    {
        /// <summary>
        /// Gets the model data for the api documentation.
        /// </summary>
        /// <returns></returns>
        public SwaggerModelData GetModelData()
        {
            return SwaggerModelData.ForType<AuthenticationSettings>(
                with =>
                {
                    with.Property(x => x.DeniedUserList)
                        .Description("The blacklisted users, this is for internal use by the application, do not modify this list.")
                        .Required(false);
                    with.Property(x => x.DeniedUsers).Description("The blacklisted users, comma separated.").Required(false);
                    with.Property(x => x.PlexAuthToken).Description("The Plex authentication token").Required(false);
                    with.Property(x => x.UsePassword)
                        .Description("Require users to use a password to login when authentication is enabled")
                        .Required(false)
                        .Default(false);
                    with.Property(x => x.UserAuthentication)
                        .Description("Require users to enter their username in, this will check against Plex")
                        .Required(false)
                        .Default(false);
                });
        }
    }
}