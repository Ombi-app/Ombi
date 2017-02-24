namespace Ombi.Services.Jobs.RecentlyAddedNewsletter
{
    public interface IPlexNewsletter
    {
        string GetNewsletterHtml(bool test);
    }
}