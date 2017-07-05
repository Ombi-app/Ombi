namespace Ombi.Notifications.Templates
{
    public interface IEmailBasicTemplate
    {
        string LoadTemplate(string subject, string body, string img);
        string TemplateLocation { get; }
    }
}