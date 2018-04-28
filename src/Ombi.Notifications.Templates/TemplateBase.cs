namespace Ombi.Notifications.Templates
{
    public abstract class TemplateBase
    {
        public abstract string TemplateLocation { get; }
        public virtual string OmbiLogo => "http://i.imgur.com/7pqVq7W.png";
    }
}