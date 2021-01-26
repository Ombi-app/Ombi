using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Ombi.Helpers;
using Ombi.Store.Context;
using Ombi.Store.Context.MySql;
using Ombi.Store.Context.Sqlite;
using SQLitePCL;

namespace Ombi.Extensions
{
    public static class DatabaseExtensions
    {

        public const string SqliteDatabase = "Sqlite";
        public const string MySqlDatabase = "MySQL";

        public static void ConfigureDatabases(this IServiceCollection services, IHealthChecksBuilder hcBuilder)
        {
            var configuration = GetDatabaseConfiguration();

            // Ombi db
            switch (configuration.OmbiDatabase.Type)
            {
                case var type when type.Equals(SqliteDatabase, StringComparison.InvariantCultureIgnoreCase):
                    services.AddDbContext<OmbiContext, OmbiSqliteContext>(x => ConfigureSqlite(x, configuration.OmbiDatabase));
                    AddSqliteHealthCheck(hcBuilder, "Ombi Database", configuration.OmbiDatabase);
                    break;
                case var type when type.Equals(MySqlDatabase, StringComparison.InvariantCultureIgnoreCase):
                    services.AddDbContext<OmbiContext, OmbiMySqlContext>(x => ConfigureMySql(x, configuration.OmbiDatabase));
                    AddMySqlHealthCheck(hcBuilder, "Ombi Database", configuration.OmbiDatabase);
                    break;
            }

            switch (configuration.ExternalDatabase.Type)
            {
                case var type when type.Equals(SqliteDatabase, StringComparison.InvariantCultureIgnoreCase):
                    services.AddDbContext<ExternalContext, ExternalSqliteContext>(x => ConfigureSqlite(x, configuration.ExternalDatabase));
                    AddSqliteHealthCheck(hcBuilder, "External Database", configuration.ExternalDatabase);
                    break;
                case var type when type.Equals(MySqlDatabase, StringComparison.InvariantCultureIgnoreCase):
                    services.AddDbContext<ExternalContext, ExternalMySqlContext>(x => ConfigureMySql(x, configuration.ExternalDatabase));
                    AddMySqlHealthCheck(hcBuilder, "External Database", configuration.ExternalDatabase);
                    break;
            }

            switch (configuration.SettingsDatabase.Type)
            {
                case var type when type.Equals(SqliteDatabase, StringComparison.InvariantCultureIgnoreCase):
                    services.AddDbContext<SettingsContext, SettingsSqliteContext>(x => ConfigureSqlite(x, configuration.SettingsDatabase));
                    AddSqliteHealthCheck(hcBuilder, "Settings Database", configuration.SettingsDatabase);
                    break;
                case var type when type.Equals(MySqlDatabase, StringComparison.InvariantCultureIgnoreCase):
                    services.AddDbContext<SettingsContext, SettingsMySqlContext>(x => ConfigureMySql(x, configuration.SettingsDatabase));
                    AddMySqlHealthCheck(hcBuilder, "Settings Database", configuration.SettingsDatabase);
                    break;
            }
        }

        public static DatabaseConfiguration GetDatabaseConfiguration()
        {
            var i = StartupSingleton.Instance;
            if (string.IsNullOrEmpty(i.StoragePath))
            {
                i.StoragePath = string.Empty;
            }

            var databaseFileLocation = Path.Combine(i.StoragePath, "database.json");

            var configuration = new DatabaseConfiguration(i.StoragePath);
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


        private static void AddSqliteHealthCheck(IHealthChecksBuilder builder, string dbName, PerDatabaseConfiguration config)
        {
            if (builder != null)
            {
                builder.AddSqlite(
                    sqliteConnectionString: config.ConnectionString,
                    name: dbName,
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new string[] { "db" });
            }
        }

        private static void AddMySqlHealthCheck(IHealthChecksBuilder builder, string dbName, PerDatabaseConfiguration config)
        {
            if (builder != null)
            {
                builder.AddMySql(
                    connectionString: config.ConnectionString,
                    name: dbName,
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new string[] { "db" }
                    );
            }
        }

        public static void ConfigureSqlite(DbContextOptionsBuilder options, PerDatabaseConfiguration config)
        {
            SQLitePCL.Batteries.Init();
            SQLitePCL.raw.sqlite3_config(raw.SQLITE_CONFIG_MULTITHREAD);
            options.UseSqlite(config.ConnectionString);
        }

        public static void ConfigureMySql(DbContextOptionsBuilder options, PerDatabaseConfiguration config)
        {
            options.UseMySql(config.ConnectionString, b =>
            {
                b.CharSetBehavior(Pomelo.EntityFrameworkCore.MySql.Infrastructure.CharSetBehavior.NeverAppend);
                b.EnableRetryOnFailure();
            });
        }

        public class DatabaseConfiguration
        {
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
    }
}