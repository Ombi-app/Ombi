namespace Ombi.Notifications.Templates
{
    public abstract class TemplateBase
    {
        public abstract string TemplateLocation { get; }
        public virtual string OmbiLogo => "https://ombi.io/img/logo-orange-small.png";
    }
}