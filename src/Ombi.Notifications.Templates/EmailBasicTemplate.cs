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
                return Path.Combine(Directory.GetCurrentDirectory(), "bin","Debug", "netcoreapp1.1","Templates", "BasicTemplate.html");
#else
                return Path.Combine(Directory.GetCurrentDirectory(), "Templates","BasicTemplate.html");
#endif
            }
        }

        

        private const string SubjectKey = "{@SUBJECT}";
        private const string BodyKey = "{@BODY}";
        private const string ImgSrc = "{@IMGSRC}";
        private const string DateKey = "{@DATENOW}";

        public string LoadTemplate(string subject, string body, string img)
        {
            try
            {
                var sb = new StringBuilder(File.ReadAllText(TemplateLocation));
                sb.Replace(SubjectKey, subject);
                sb.Replace(BodyKey, body);
                sb.Replace(DateKey, DateTime.Now.ToString("f"));
                sb.Replace(ImgSrc, img);

                return sb.ToString();
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }
    }
}
