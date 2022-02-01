using System;
using System.IO;
using System.Text;
using Ombi.I18n.Resources;

namespace Ombi.Notifications.Templates
{
    public class NewsletterTemplate : TemplateBase, INewsletterTemplate
    {
        public override string TemplateLocation
        {
            get
            {
                if (string.IsNullOrEmpty(_templateLocation))
                {
#if DEBUG
                    _templateLocation = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Debug", "net6.0", "Templates", "NewsletterTemplate.html");
#else
                    _templateLocation = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "NewsletterTemplate.html");
#endif
                }
                return _templateLocation;
            }
        }

        private string _templateLocation;
        
        private const string SubjectKey = "{@SUBJECT}";
        private const string DateKey = "{@DATENOW}";
        private const string Logo = "{@LOGO}";
        private const string TableLocation = "{@RECENTLYADDED}";
        private const string IntroText = "{@INTRO}";
        private const string Unsubscribe = "{@UNSUBSCRIBE}";
        private const string UnsubscribeText = "{@UNSUBSCRIBETEXT}";
        private const string PoweredByText = "{@POWEREDBYTEXT}";


        public string LoadTemplate(string subject, string intro, string tableHtml, string logo, string unsubscribeLink)
        {
            var sb = new StringBuilder(File.ReadAllText(TemplateLocation));
            sb.Replace(SubjectKey, subject);
            sb.Replace(TableLocation, tableHtml);
            sb.Replace(IntroText, intro);
            sb.Replace(DateKey, DateTime.Now.ToString("f"));
            sb.Replace(Logo, string.IsNullOrEmpty(logo) ? OmbiLogo : logo);
            sb.Replace(Unsubscribe, string.IsNullOrEmpty(unsubscribeLink) ? string.Empty : unsubscribeLink);
            sb.Replace(UnsubscribeText, string.IsNullOrEmpty(unsubscribeLink) ? string.Empty : "Unsubscrible");
            sb.Replace(PoweredByText, Texts.PoweredBy);

            return sb.ToString();
        }
    }
}
