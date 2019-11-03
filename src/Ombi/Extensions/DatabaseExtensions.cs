using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Ombi.Helpers;
using Ombi.Store.Context;
using SQLitePCL;

namespace Ombi.Extensions
{
    public static class DatabaseExtensions
    {

        private const string SqliteDatabase = "Sqlite";
        private const string MySqlDatabase = "MySQL";

        public static void ConfigureDatabases(this IServiceCollection services)
        {
            var i = StoragePathSingleton.Instance;
            if (string.IsNullOrEmpty(i.StoragePath))
            {
                i.StoragePath = string.Empty;
            }

            var databaseFileLocation = Path.Combine(i.StoragePath, "database.json");
            var configuration = GetDatabaseConfiguration(databaseFileLocation, i.StoragePath);

            // Ombi db
            switch (configuration.OmbiDatabase.Type)
            {
                case var type when type.Equals(SqliteDatabase, StringComparison.InvariantCultureIgnoreCase):
                    services.AddDbContext<OmbiContext, OmbiSqliteContext>(x => ConfigureSqlite(x, configuration.OmbiDatabase));
                    break;
                case var type when type.Equals(MySqlDatabase, StringComparison.InvariantCultureIgnoreCase):
                    services.AddDbContext<OmbiContext, OmbiMySqlContext>(x => ConfigureMySql(x, configuration.OmbiDatabase));
                    break;
            }
        }

        private static DatabaseConfiguration GetDatabaseConfiguration(string databaseFileLocation, string storagePath)
        {
            var configuration = new DatabaseConfiguration(storagePath);
            if (File.Exists(databaseFileLocation))
            {
                var databaseJson = File.ReadAllText(databaseFileLocation);
                if (string.IsNullOrEmpty(databaseJson))
                {
                    throw new Exception("Found 'database.json' but it was empty!");
                }

                configuration = JsonConvert.DeserializeObject<DatabaseConfiguration>(databaseJson);
            }

            return configuration;
        }

        private static void ConfigureSqlite(DbContextOptionsBuilder options, PerDatabaseConfiguration config)
        {
            SQLitePCL.Batteries.Init();
            SQLitePCL.raw.sqlite3_config(raw.SQLITE_CONFIG_MULTITHREAD);

            options.UseSqlite(config.ConnectionString);
        }

        private static void ConfigureMySql(DbContextOptionsBuilder options, PerDatabaseConfiguration config)
        {
            options.UseMySql(config.ConnectionString);
        }

        private class DatabaseConfiguration
        {
            public DatabaseConfiguration()
            {
                
            }

            public DatabaseConfiguration(string defaultSqlitePath)
            {
                OmbiDatabase = new PerDatabaseConfiguration(SqliteDatabase, $"Data Source={Path.Combine(defaultSqlitePath, "Ombi.db")}");
                SettingsDatabase = new PerDatabaseConfiguration(SqliteDatabase, $"Data Source={Path.Combine(defaultSqlitePath, "Settings.db")}");
                ExternalDatabase = new PerDatabaseConfiguration(SqliteDatabase, $"Data Source={Path.Combine(defaultSqlitePath, "External.db")}");
            }
            public PerDatabaseConfiguration OmbiDatabase { get; set; }
            public PerDatabaseConfiguration SettingsDatabase { get; set; }
            public PerDatabaseConfiguration ExternalDatabase { get; set; }
        }

        private class PerDatabaseConfiguration
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
    }
}