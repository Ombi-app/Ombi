namespace Ombi.Notifications.Templates
{
    public interface IEmailBasicTemplate
    {
        string LoadTemplate(string subject, string body, string img = default(string), string logo = default(string), string url = default(string));
        string TemplateLocation { get; }
    }
}