namespace Ombi.Notifications.Templates
{
    public interface IEmailBasicTemplate
    {
        string LoadTemplate(string subject, string body, string img = default(string), string logo = default(string), string url = default(string));
        string LoadTemplate(string subject, string body, string img = default(string), string logo = default(string), string url = default(string), bool includeLogo = true, bool includePoster = true);
        string TemplateLocation { get; }
    }
}