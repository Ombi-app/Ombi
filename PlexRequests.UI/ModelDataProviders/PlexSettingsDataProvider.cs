#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: PlexSettingsDataProvider.cs
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
using Ombi.Core.SettingModels;

namespace Ombi.UI.ModelDataProviders
{
    public class PlexSettingsDataProvider : ISwaggerModelDataProvider
    {
        /// <summary>
        /// Gets the model data for the api documentation.
        /// </summary>
        /// <returns></returns>
        public SwaggerModelData GetModelData()
        {
            return SwaggerModelData.ForType<PlexSettings>(
                with =>
                {
                    with.Property(x => x.Ip).Description("The IP address of the Plex Server").Required(true);
                    with.Property(x => x.Port).Description("The Port address of the Plex Server").Required(true).Default(32400);
                    with.Property(x => x.Ssl).Description("Enable SSL").Required(false).Default(false);
                    with.Property(x => x.FullUri).Description("Internal Property").Required(false);

                    with.Property(x => x.SubDir).Description("Subdir/BaseUrl of Plex").Required(false);
                });
        }
    }
}