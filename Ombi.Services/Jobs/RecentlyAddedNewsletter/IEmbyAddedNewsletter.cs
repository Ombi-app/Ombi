namespace Ombi.Services.Jobs.RecentlyAddedNewsletter
{
    public interface IEmbyAddedNewsletter
    {
        string GetNewsletterHtml(bool test);
    }
}