using System;
using System.IO;
using System.Text;

namespace Ombi.Notifications.Templates
{
    public class EmailBasicTemplate : IEmailBasicTemplate
    {
        public string TemplateLocation
        {
            get
            {
#if DEBUG
                return Path.Combine(Directory.GetCurrentDirectory(), "bin", "Debug", "netcoreapp1.1", "Templates", "BasicTemplate.html");
#else
                return Path.Combine(Directory.GetCurrentDirectory(), "Templates","BasicTemplate.html");
#endif
            }
        }
        
        private const string SubjectKey = "{@SUBJECT}";
        private const string BodyKey = "{@BODY}";
        private const string ImgSrc = "{@IMGSRC}";
        private const string DateKey = "{@DATENOW}";
        private const string Logo = "{@DATENOW}";

        public string LoadTemplate(string subject, string body, string img, string logo)
        {
            var sb = new StringBuilder(File.ReadAllText(TemplateLocation));
            sb.Replace(SubjectKey, subject);
            sb.Replace(BodyKey, body);
            sb.Replace(DateKey, DateTime.Now.ToString("f"));
            sb.Replace(ImgSrc, img);
            sb.Replace(Logo, string.IsNullOrEmpty(logo) ? "http://i.imgur.com/qQsN78U.png" : logo);

            return sb.ToString();
        }
    }
}
