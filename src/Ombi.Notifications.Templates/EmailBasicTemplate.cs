using System;
using System.IO;
using System.Text;

namespace Ombi.Notifications.Templates
{
    public class EmailBasicTemplate : IEmailBasicTemplate
    {
        public string TemplateLocation => Path.Combine(Directory.GetCurrentDirectory(), "Templates","BasicTemplate.html");

        private const string SubjectKey = "{@SUBJECT}";
        private const string BodyKey = "{@BODY}";
        private const string ImgSrc = "{@IMGSRC}";
        private const string DateKey = "{@DATENOW}";

        public string LoadTemplate(string subject, string body, string imgSrc)
        {
            try
            {
                var sb = new StringBuilder(File.ReadAllText(TemplateLocation));
                sb.Replace(SubjectKey, subject);
                sb.Replace(BodyKey, body);
                sb.Replace(ImgSrc, imgSrc);
                sb.Replace(DateKey, DateTime.Now.ToString("f"));

                return sb.ToString();
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }
    }
}
