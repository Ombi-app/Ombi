#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: AssetHelper.cs
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
using System.Text;

using Nancy.ViewEngines.Razor;

using PlexRequests.Core;
using PlexRequests.Core.SettingModels;

namespace PlexRequests.UI.Helpers
{
    public static class AssetHelper
    {
        private static ServiceLocator Locator => ServiceLocator.Instance;
        public static IHtmlString LoadAssets(this HtmlHelpers helper)
        {
            var settings = Locator.Resolve<ISettingsService<PlexRequestSettings>>().GetSettings();
            var sb = new StringBuilder();
            var assetLocation = settings.BaseUrl;

            var content = GetContentUrl(assetLocation);

            sb.AppendLine($"<link rel=\"stylesheet\" href=\"{content}/Content/bootstrap.css\" type=\"text/css\"/>");
            sb.AppendLine($"<link rel=\"stylesheet\" href=\"{content}/Content/custom.min.css\" type=\"text/css\" />");
            sb.AppendLine($"<link rel=\"stylesheet\" href=\"{content}/Content/font-awesome.css\" type=\"text/css\"/>");
            sb.AppendLine($"<link rel=\"stylesheet\" href=\"{content}/Content/pace.min.css\" type=\"text/css\"/>");

            sb.AppendLine($"<script src=\"{content}/Content/jquery-2.2.1.min.js\"></script>");
            sb.AppendLine($"<script src=\"{content}/Content/handlebars.min.js\"></script>");
            sb.AppendLine($"<script src=\"{content}/Content/bootstrap.min.js\"></script>");
            sb.AppendLine($"<script src=\"{content}/Content/site.js\"></script>");
            sb.AppendLine($"<script src=\"{content}/Content/pace.min.js\"></script>");
            sb.AppendLine($"<script src=\"{content}/Content/jquery.mixitup.js\"></script>");
            sb.AppendLine($"<script src=\"{content}/Content/moment.min.js\"></script>");


            return helper.Raw(sb.ToString());
        }

        public static IHtmlString LoadSearchAssets(this HtmlHelpers helper)
        {
            var settings = Locator.Resolve<ISettingsService<PlexRequestSettings>>().GetSettings();
            var sb = new StringBuilder();
            var assetLocation = settings.BaseUrl;

            var content = GetContentUrl(assetLocation);

            sb.AppendLine($"<script src=\"{content}/Content/search.js\" type=\"text/javascript\"></script>");

            return helper.Raw(sb.ToString());
        }

        public static IHtmlString LoadRequestAssets(this HtmlHelpers helper)
        {
            var settings = Locator.Resolve<ISettingsService<PlexRequestSettings>>().GetSettings();
            var sb = new StringBuilder();
            var assetLocation = settings.BaseUrl;

            var content = GetContentUrl(assetLocation);

            sb.AppendLine($"<script src=\"{content}/Content/requests.js\" type=\"text/javascript\"></script>");

            return helper.Raw(sb.ToString());
        }

        public static IHtmlString LoadLogsAssets(this HtmlHelpers helper)
        {
            var settings = Locator.Resolve<ISettingsService<PlexRequestSettings>>().GetSettings();
            var sb = new StringBuilder();
            var assetLocation = settings.BaseUrl;

            var content = GetContentUrl(assetLocation);

            sb.AppendLine($"<script src=\"{content}/Content/datatables.min.js\" type=\"text/javascript\"></script>");
            sb.AppendLine($"<link rel=\"stylesheet\" type=\"text/css\" href=\"{content}/Content/dataTables.bootstrap.css\" />");

            return helper.Raw(sb.ToString());
        }
        
        private static string GetContentUrl(string assetLocation)
        {
            return string.IsNullOrEmpty(assetLocation) ? string.Empty : $"/{assetLocation}";
        }
    }
}