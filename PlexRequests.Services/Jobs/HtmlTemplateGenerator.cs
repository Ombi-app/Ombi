#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: HtmlTemplateGenerator.cs
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

using System.IO;
using System.Text;
using System.Web.UI;

namespace PlexRequests.Services.Jobs
{
    public abstract class HtmlTemplateGenerator
    {
        protected virtual void AddParagraph(StringBuilder stringBuilder, string text, int fontSize = 14, string fontWeight = "normal")
        {
            stringBuilder.AppendFormat("<p style=\"font-family: sans-serif; font-size: {1}px; font-weight: {2}; margin: 0; Margin-bottom: 15px;\">{0}</p>", text, fontSize, fontWeight);
        }

        protected virtual void AddImageInsideTable(StringBuilder sb, string url)
        {
            sb.Append("<tr>");
            sb.Append("<td align=\"center\">");
            sb.AppendFormat(
                "<img src=\"{0}\" width=\"400px\" text-align=\"center\" />",
                url);
            sb.Append("</td>");
            sb.Append("</tr>");
        }

        protected virtual void Href(StringBuilder sb, string url)
        {
            sb.AppendFormat("<a href=\"{0}\">", url);
        }

        protected virtual void EndTag(StringBuilder sb, string tag)
        {
            sb.AppendFormat("</{0}>", tag);
        }

        protected virtual void Header(StringBuilder sb, int size, string text, string fontWeight = "normal")
        {
            sb.AppendFormat(
                "<h{0} style=\"font-family: sans-serif; font-weight: {2}; margin: 0; Margin-bottom: 15px;\">{1}</h{0}>",
                size, text, fontWeight);
        }


    }
}