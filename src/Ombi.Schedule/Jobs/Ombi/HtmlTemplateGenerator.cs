using System.Text;
using Ombi.Helpers;

namespace Ombi.Schedule.Jobs.Ombi
{
    public abstract class HtmlTemplateGenerator
    {
        protected virtual void AddBackgroundInsideTable(StringBuilder sb, string url)
        {
            sb.Append("<td align=\"center\" valign=\"top\" width=\"500\" height=\"252\" class=\"media-card\" style=\"font-size: 14px; font-family: 'Open Sans', Helvetica, Arial, sans-serif; vertical-align: top; padding: 3px; width: 500px; min-width: 500px; max-width: 500px; height: 252px; max-height: 252px; \">");
            sb.AppendFormat("<table class=\"card-bg\" width=\"500\" height=\"252\" background=\"url(0)\" bgcolor=\"#1f1f1f\" style=\"background-image: url(0); border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 500px; background-color: #1f1f1f; background-position: center; background-size: cover; background-repeat: no-repeat; background-clip: padding-box; border: 2px solid rgba(255, 118, 27, .4); height: 252px; max-height: 252px; \">", url);
            sb.Append("<tr>");
            sb.Append("<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\" valign=\"top\">");
            sb.Append("<table class=\"bg-tint\" width=\"100%\" bgcolor=\"rgba(0, 0, 0, .6)\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; background-color: rgba(0, 0, 0, .6); \">");
        }

        protected virtual void AddPosterInsideTable(StringBuilder sb, string url)
        {
            sb.Append("<tr>");
            sb.Append("<td class=\"poster-container\" width=\"150\" height=\"225\" valign=\"top\" style=\"ont-family: sans-serif; font-size: 14px; vertical-align: top; width: 150px; min-width: 150px; height: 225px; max-height: 225px; min-height: 225px; \">");
            sb.AppendFormat("<table class=\"poster-img\" width=\"100%\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; \">", url);
        }

        protected virtual void AddMediaServerUrl(StringBuilder sb, string mediaurl, string url)
        {
            if (url.HasValue())
            {
                sb.Append("<tr>");
                sb.Append(
                    "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; \">");
                sb.AppendFormat("<a href=\"{0}\" target=\"_blank\">", mediaurl);
                sb.AppendFormat(
                    "<img class=\"poster-overlay\" src=\"{0}\" width=\"150\" height=\"225\" style=\"border: none; -ms-interpolation-mode: bicubic; max-width: 100%; \">",
                    url);
                sb.Append("</a>");
                sb.Append("</td>");
                sb.Append("</tr>");
            }

            sb.Append("</table>");
            sb.Append("</td>");
        }

        protected virtual void AddInfoTable(StringBuilder sb)
        {
            sb.Append(
                "<td class=\"movie-info\" height=\"227\" valign=\"top\" align=\"left\" style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-size: 14px; vertical-align: top; padding-left: 4px; text-align: left; height: 227px; \">");
            sb.Append("<table style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; height: 100%; \">");
        }

        protected virtual void AddTitle(StringBuilder sb, string url, string title)
        {
            sb.Append("<tr class=\"title\" valign=\"top\" style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif;font-size: 22px;line-height: 24px;vertical-align: top;max-width: 320px;display: block;height: 50px;min-height: 50px;max-height: 50px; \">");
            sb.Append("<td>");
            if(url.HasValue()) sb.AppendFormat("<a href=\"{0}\" target=\"_blank\" style=\"text-decoration: none; font-weight: 400; color: #ff761b;\">", url);
            sb.AppendFormat("<h1 style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif;font-size: 22px;line-height: 24px;vertical-align: top;max-width: 320px;display: block;height: 50px;min-height: 50px;max-height: 50px;\" >{0}</h1>", title);
            if (url.HasValue()) sb.Append("</a>");
            sb.Append("</td>");
            sb.Append("</tr>");
        }

        protected virtual void AddParagraph(StringBuilder sb, string text)
        {
            sb.Append("<tr class=\"description\" style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif;height: 130px;max-height: 130px;max-width: 320px;overflow: hidden;text-overflow: ellipsis;display: block;font-size: 14px !important;text-align: justify;\" valign=\"top\">");
            sb.Append("<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; \">");
            sb.AppendFormat("<p style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-weight: normal; margin: 0; margin-bottom: 15px; \">{0}</p>", text);
            sb.Append("</td>");
            sb.Append("</tr>");
        }

        protected virtual void AddTvParagraph(StringBuilder sb, string episodes, string summary)
        {
            sb.Append("<tr class=\"description\" style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif;height: 130px;max-height: 130px;max-width: 320px;overflow: hidden;text-overflow: ellipsis;display: block;font-size: 14px !important;text-align: justify;\" valign=\"top\">");
            sb.Append("<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; \">");
            sb.AppendFormat("<p style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif; font-weight: normal; margin: 0; margin-bottom: 15px; \">{0}</p>", episodes);
            sb.AppendFormat("<div>{0}</div>", summary);
            sb.Append("</td>");
            sb.Append("</tr>");
        }

        protected virtual void AddGenres(StringBuilder sb, string text)
        {
            sb.Append("<tr class=\"meta\" style=\"font-family: 'Open Sans', Helvetica, Arial, sans-serif; max-width: 300px; min-width: 300px; padding: 3px 7px; margin-top: 10px; line-height: 1; text-align: left; white-space: nowrap; vertical-align: middle; background-color: rgba(255, 118, 27, 0.5); color: #fff; border-radius: 2px; overflow: hidden; display: block; font-size: 0.9rem;\" align=\"left\" valign=\"middle\" bgcolor=\"rgba(255, 118, 27, 0.5)\">");
            sb.Append("<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; \">");
            sb.AppendFormat("<span>{0}</span>", text);
            sb.Append("</td>");
            sb.Append("</tr>");
        }
    }
}
