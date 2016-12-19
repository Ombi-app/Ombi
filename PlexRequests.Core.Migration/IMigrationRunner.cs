namespace Ombi.Core.Migration
{
    public interface IMigrationRunner
    {
        void MigrateToLatest();
    }
}