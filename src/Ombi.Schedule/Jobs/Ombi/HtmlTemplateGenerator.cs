using System.Text;

namespace Ombi.Schedule.Jobs.Ombi
{
    public abstract class HtmlTemplateGenerator
    {
        protected virtual void AddParagraph(StringBuilder stringBuilder, string text, int fontSize = 14, string fontWeight = "normal")
        {
            stringBuilder.AppendFormat("<p style=\"font-family: sans-serif; font-size: {1}px; font-weight: {2}; margin: 0; Margin-bottom: 15px;\">{0}</p>", text, fontSize, fontWeight);
        }

        protected virtual void AddImageInsideTable(StringBuilder sb, string url, int size = 400)
        {
            sb.Append("<tr>");
            sb.Append("<td align=\"center\">");
            sb.Append($"<img src=\"{url}\" width=\"{size}px\" text-align=\"center\" />");
            sb.Append("</td>");
            sb.Append("</tr>");
        }

        protected virtual void Href(StringBuilder sb, string url)
        {
            sb.AppendFormat("<a href=\"{0}\">", url);
        }

        protected virtual void TableData(StringBuilder sb)
        {
            sb.Append(
                "<td align=\"center\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\" valign=\"top\">");
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