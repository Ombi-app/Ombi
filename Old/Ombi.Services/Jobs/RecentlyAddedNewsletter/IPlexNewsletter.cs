namespace Ombi.Services.Jobs.RecentlyAddedNewsletter
{
    public interface IPlexNewsletter
    {
        Newsletter GetNewsletter(bool test);
    }
}