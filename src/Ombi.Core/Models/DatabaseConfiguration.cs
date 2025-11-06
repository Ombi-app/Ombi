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
        OmbiDatabase = new PerDatabaseConfiguration(SqliteDatabase, $"Data Source={Path.Combine(defaultSqlitePath, "Ombi.db")};Mode=ReadWriteCreate;Journal Mode=WAL;Cache=Shared;Pooling=True;Timeout=30000");
        SettingsDatabase = new PerDatabaseConfiguration(SqliteDatabase, $"Data Source={Path.Combine(defaultSqlitePath, "OmbiSettings.db")};Mode=ReadWriteCreate;Journal Mode=WAL;Cache=Shared;Pooling=True;Timeout=30000");
        ExternalDatabase = new PerDatabaseConfiguration(SqliteDatabase, $"Data Source={Path.Combine(defaultSqlitePath, "OmbiExternal.db")};Mode=ReadWriteCreate;Journal Mode=WAL;Cache=Shared;Pooling=True;Timeout=30000");
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