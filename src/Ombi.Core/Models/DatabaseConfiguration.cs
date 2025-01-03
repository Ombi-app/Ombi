using System.IO;

namespace Ombi.Core.Models;

public class DatabaseConfiguration
{
    public const string SqliteDatabase = "Sqlite";

    public DatabaseConfiguration()
    {

    }

    public DatabaseConfiguration(string defaultSqlitePath)
    {
        OmbiDatabase = new PerDatabaseConfiguration(SqliteDatabase, $"Data Source={Path.Combine(defaultSqlitePath, "Ombi.db")}");
        SettingsDatabase = new PerDatabaseConfiguration(SqliteDatabase, $"Data Source={Path.Combine(defaultSqlitePath, "OmbiSettings.db")}");
        ExternalDatabase = new PerDatabaseConfiguration(SqliteDatabase, $"Data Source={Path.Combine(defaultSqlitePath, "OmbiExternal.db")}");
    }
    public PerDatabaseConfiguration OmbiDatabase { get; set; }
    public PerDatabaseConfiguration SettingsDatabase { get; set; }
    public PerDatabaseConfiguration ExternalDatabase { get; set; }
}

public class PerDatabaseConfiguration
{
    public PerDatabaseConfiguration(string type, string connectionString)
    {
        Type = type;
        ConnectionString = connectionString;
    }

    // Used in Deserialization
    public PerDatabaseConfiguration()
    {

    }
    public string Type { get; set; }
    public string ConnectionString { get; set; }
}