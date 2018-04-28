using System.Text;

namespace Ombi.Schedule.Jobs.Ombi
{
    public abstract class HtmlTemplateGenerator
    {
        protected virtual void AddBackgroundInsideTable(StringBuilder sb, string url)
        {
            sb.Append("<td align=\"center\" valign=\"top\" class=\"media-card\" style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-size: 12px; vertical-align: top; padding: 3px; width: 502px; min-width: 500px; max-width: 500px; height: 235px; \">");
            sb.AppendFormat("<table class=\"card-bg\" style=\"background-image: url({0}); border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; background-color: #1f1f1f; background-position: center; background-size: cover; background-repeat: no-repeat; background-clip: padding-box; border: 2px solid rgba(255,118,27,.4); \">", url);
            sb.Append("<tr>");
            sb.Append("<td>");
            sb.Append("<table class=\"bg-tint\" style=\"background-color: rgba(0, 0, 0, .6); position: absolute; width: 490px; height: 239px; \">");
        }

        protected virtual void AddPosterInsideTable(StringBuilder sb, string url)
        {
            sb.Append("<tr>");
            sb.Append("<td class=\"poster-container\" style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-size: 14px; vertical-align: top; width: 150px; min-width: 15px; height: 225px; \">");
            sb.AppendFormat("<table class=\"poster-img\" style=\"background-image: url({0}); border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; background-color: transparent; background-position: center; background-size: cover; background-repeat: no-repeat; background-clip: padding-box; border: 1px solid rgba(255,255,255,.1); \">", url);
        }

        protected virtual void AddMediaServerUrl(StringBuilder sb, string mediaurl, string url)
        {
            sb.Append("<tr>");
            sb.Append("<td style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-size: 14px; vertical-align: top; \">");
            sb.AppendFormat("<a href=\"{0}\" target=\"_blank\">", mediaurl);
            sb.AppendFormat("<img class=\"poster-overlay\" src=\"{0}\" width=\"150\" height=\"225\" style=\"border: none;-ms-interpolation-mode: bicubic; max-width: 100%;display: block; visibility: hidden; \">", url);
            sb.Append("</a>");
            sb.Append("</td>");
            sb.Append("</tr>");
            sb.Append("</table>");
            sb.Append("</td>");
        }

        protected virtual void AddInfoTable(StringBuilder sb)
        {
            sb.Append(
                "<td class=\"movie-info\" style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-size: 14px; vertical-align: top; padding-left: 4px; text-align: left; height: 227px; \">");
            sb.Append("<table style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; height: 100%; \">");
        }

        protected virtual void AddTitle(StringBuilder sb, string url, string title)
        {
            sb.Append("<tr>");
            sb.Append("<td class=\"title\" style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-size: 0.9rem; vertical-align: top; white-space: nowrap; text-overflow: ellipsis; overflow: hidden; line-height: 1.2rem; padding: 5px; \">");
            sb.AppendFormat("<a href=\"{0}\" target=\"_blank\">", url);
            sb.AppendFormat("<h1 style=\"white-space: normal; line-height: 1;\" >{0}</h1>", title);
            sb.Append("</a>");
            sb.Append("</td>");
            sb.Append("</tr>");
        }

        protected virtual void AddParagraph(StringBuilder sb, string text)
        {
            sb.Append("<tr class=\"description\">");
            sb.Append("<td style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-size: 0.75rem; vertical-align: top; padding: 5px; height: 100%; \">");
            sb.AppendFormat("<p style=\"color: #fff; font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-weight: 400; margin: 0; max-width: 325px; text-align: justify; \">{0}</p>", text);
            sb.Append("</td>");
            sb.Append("</tr>");
        }

        protected virtual void AddTvParagraph(StringBuilder sb, string episodes, string summary)
        {
            sb.Append("<tr class=\"description\">");
            sb.Append("<td style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-size: 0.75rem; vertical-align: top; padding: 5px; height: 100%; \">");
            sb.AppendFormat("<p style=\"color: #fff; font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-weight: 400; margin: 0; max-width: 325px; margin-bottom: 10px; \">{0}</p>", episodes);
            sb.AppendFormat("<div style=\"color: #fff; font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-weight: 400; margin: 0; max-width: 325px; overflow: hidden; text-align: justify; \">{0}</div>", summary);
            sb.Append("</td>");
            sb.Append("</tr>");
        }

        protected virtual void AddGenres(StringBuilder sb, string text)
        {
            sb.Append("<tr class=\"meta\">");
            sb.Append("<td style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-size: 14px; vertical-align: top; max-width: 265px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; \">");
            sb.AppendFormat("<span style=\"display: inline-block; min-width: 10px; padding: 3px 7px; font-size: 11px; line-height: 1; text-align: center; white-space: nowrap; vertical-align: middle; background-color: rgba(255, 118, 27, 0.5); color: #fff; border-radius: 2px; text-overflow: ellipsis; overflow: hidden; \">{0}</span>", text);
            sb.Append("</td>");
            sb.Append("</tr>");
        }
    }
}