namespace Ombi.Services.Jobs.RecentlyAddedNewsletter
{
    public interface IEmbyAddedNewsletter
    {
        Newsletter GetNewsletter(bool test);
    }
}