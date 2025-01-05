using System;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySqlConnector;
using Newtonsoft.Json;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Ombi.Core.Helpers;
using Ombi.Core.Models;
using Ombi.Helpers;
using Ombi.Store.Context;
using Ombi.Store.Context.MySql;
using Ombi.Store.Context.Sqlite;
using Ombi.Store.Context.Postgres;
using Polly;
using Pomelo.EntityFrameworkCore.MySql.Storage.Internal;
using SQLitePCL;

namespace Ombi.Extensions
{
    public static class DatabaseExtensions
    {

        public const string SqliteDatabase = "Sqlite";
        public const string MySqlDatabase = "MySQL";
        public const string PostgresDatabase = "Postgres";

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
                    services.AddDbContext<OmbiContext, OmbiMySqlContext>(x => DatabaseConfigurationSetup.ConfigureMySql(x, configuration.OmbiDatabase));
                    AddMySqlHealthCheck(hcBuilder, "Ombi Database", configuration.OmbiDatabase);
                    break;
                case var type when type.Equals(PostgresDatabase, StringComparison.InvariantCultureIgnoreCase):
                    services.AddDbContext<OmbiContext, OmbiPostgresContext>(x => DatabaseConfigurationSetup.ConfigurePostgres(x, configuration.OmbiDatabase));
                    AddPostgresHealthCheck(hcBuilder, "Ombi Database", configuration.OmbiDatabase);
                    break;
            }

            switch (configuration.ExternalDatabase.Type)
            {
                case var type when type.Equals(SqliteDatabase, StringComparison.InvariantCultureIgnoreCase):
                    services.AddDbContext<ExternalContext, ExternalSqliteContext>(x => ConfigureSqlite(x, configuration.ExternalDatabase));
                    AddSqliteHealthCheck(hcBuilder, "External Database", configuration.ExternalDatabase);
                    break;
                case var type when type.Equals(MySqlDatabase, StringComparison.InvariantCultureIgnoreCase):
                    services.AddDbContext<ExternalContext, ExternalMySqlContext>(x => DatabaseConfigurationSetup.ConfigureMySql(x, configuration.ExternalDatabase));
                    AddMySqlHealthCheck(hcBuilder, "External Database", configuration.ExternalDatabase);
                    break;
                case var type when type.Equals(PostgresDatabase, StringComparison.InvariantCultureIgnoreCase):
                    services.AddDbContext<ExternalContext, ExternalPostgresContext>(x => DatabaseConfigurationSetup.ConfigurePostgres(x, configuration.ExternalDatabase));
                    AddPostgresHealthCheck(hcBuilder, "External Database", configuration.ExternalDatabase);
                    break;
            }

            switch (configuration.SettingsDatabase.Type)
            {
                case var type when type.Equals(SqliteDatabase, StringComparison.InvariantCultureIgnoreCase):
                    services.AddDbContext<SettingsContext, SettingsSqliteContext>(x => ConfigureSqlite(x, configuration.SettingsDatabase));
                    AddSqliteHealthCheck(hcBuilder, "Settings Database", configuration.SettingsDatabase);
                    break;
                case var type when type.Equals(MySqlDatabase, StringComparison.InvariantCultureIgnoreCase):
                    services.AddDbContext<SettingsContext, SettingsMySqlContext>(x => DatabaseConfigurationSetup.ConfigureMySql(x, configuration.SettingsDatabase));
                    AddMySqlHealthCheck(hcBuilder, "Settings Database", configuration.SettingsDatabase);
                    break;
                case var type when type.Equals(PostgresDatabase, StringComparison.InvariantCultureIgnoreCase):
                    services.AddDbContext<SettingsContext, SettingsPostgresContext>(x => DatabaseConfigurationSetup.ConfigurePostgres(x, configuration.SettingsDatabase));
                    AddPostgresHealthCheck(hcBuilder, "Settings Database", configuration.SettingsDatabase);
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

        private static void AddPostgresHealthCheck(IHealthChecksBuilder builder, string dbName, PerDatabaseConfiguration config)
        {
            if (builder != null)
            {
                builder.AddNpgSql(
                    npgsqlConnectionString: config.ConnectionString,
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
    }
}
