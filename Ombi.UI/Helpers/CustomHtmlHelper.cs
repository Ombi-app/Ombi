#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: CustomHtmlHelper.cs
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
using Ombi.Helpers;

namespace Ombi.UI.Helpers
{
    public static class CustomHtmlHelper
    {
        public static IHtmlString GetInformationalVersion(this HtmlHelpers helper)
        {
            var fileVersion = AssemblyHelper.GetAssemblyVersion();
            var htmlString = $"<!--\r\n##################################################################\r\nVersion: {fileVersion}\r\n##################################################################\r\n--> ";

            return helper.Raw(htmlString);
        }

        public static IHtmlString Checkbox(this HtmlHelpers helper, bool check, string name, string display)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<div class=\"form-group\">");
            sb.AppendLine("<div class=\"checkbox\">");
            sb.AppendFormat("<input type=\"checkbox\" id=\"{0}\" name=\"{0}\" {2}><label for=\"{0}\">{1}</label>", name, display, check ? "checked=\"checked\"" : string.Empty);
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            return helper.Raw(sb.ToString());
        }
    }
}