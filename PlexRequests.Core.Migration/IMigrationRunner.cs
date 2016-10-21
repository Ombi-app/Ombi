namespace PlexRequests.Core.Migration
{
    public interface IMigrationRunner
    {
        void MigrateToLatest();
    }
}