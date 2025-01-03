using System.Threading;
using System.Threading.Tasks;

namespace Ombi.Core.Services;

public interface IDatabaseConfigurationService
{
    const string MySqlDatabase = "MySQL";
    const string PostgresDatabase = "Postgres";
    Task<bool> ConfigureDatabase(string databaseType, string connectionString, CancellationToken token);
}