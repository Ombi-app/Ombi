﻿using System;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySqlConnector;
using Newtonsoft.Json;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
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
                    services.AddDbContext<OmbiContext, OmbiMySqlContext>(x => ConfigureMySql(x, configuration.OmbiDatabase));
                    AddMySqlHealthCheck(hcBuilder, "Ombi Database", configuration.OmbiDatabase);
                    break;
                case var type when type.Equals(PostgresDatabase, StringComparison.InvariantCultureIgnoreCase):
                    services.AddDbContext<OmbiContext, OmbiPostgresContext>(x => ConfigurePostgres(x, configuration.OmbiDatabase));
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
                    services.AddDbContext<ExternalContext, ExternalMySqlContext>(x => ConfigureMySql(x, configuration.ExternalDatabase));
                    AddMySqlHealthCheck(hcBuilder, "External Database", configuration.ExternalDatabase);
                    break;
                case var type when type.Equals(PostgresDatabase, StringComparison.InvariantCultureIgnoreCase):
                    services.AddDbContext<ExternalContext, ExternalPostgresContext>(x => ConfigurePostgres(x, configuration.ExternalDatabase));
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
                    services.AddDbContext<SettingsContext, SettingsMySqlContext>(x => ConfigureMySql(x, configuration.SettingsDatabase));
                    AddMySqlHealthCheck(hcBuilder, "Settings Database", configuration.SettingsDatabase);
                    break;
                case var type when type.Equals(PostgresDatabase, StringComparison.InvariantCultureIgnoreCase):
                    services.AddDbContext<SettingsContext, SettingsPostgresContext>(x => ConfigurePostgres(x, configuration.SettingsDatabase));
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

        public static void ConfigureMySql(DbContextOptionsBuilder options, PerDatabaseConfiguration config)
        {
            if (string.IsNullOrEmpty(config.ConnectionString))
            {
                throw new ArgumentNullException("ConnectionString for the MySql/Mariadb database is empty");
            }

            options.UseMySql(config.ConnectionString, GetServerVersion(config.ConnectionString), b =>
            {
                //b.CharSetBehavior(Pomelo.EntityFrameworkCore.MySql.Infrastructure.CharSetBehavior.NeverAppend); // ##ISSUE, link to migrations?
                b.EnableRetryOnFailure();
            });
        }

        public static void ConfigurePostgres(DbContextOptionsBuilder options, PerDatabaseConfiguration config)
        {
            options.UseNpgsql(config.ConnectionString, b =>
            {
                b.EnableRetryOnFailure();
            }).ReplaceService<ISqlGenerationHelper, NpgsqlCaseInsensitiveSqlGenerationHelper>();
        }

        private static ServerVersion GetServerVersion(string connectionString)
        {
            // Workaround Windows bug, that can lead to the following exception:
            //
            // MySqlConnector.MySqlException (0x80004005): SSL Authentication Error
            //     ---> System.Security.Authentication.AuthenticationException: Authentication failed, see inner exception.
            //     ---> System.ComponentModel.Win32Exception (0x8009030F): The message or signature supplied for verification has been altered
            //
            // See https://github.com/dotnet/runtime/issues/17005#issuecomment-305848835
            //
            // Also workaround for the fact, that ServerVersion.AutoDetect() does not use any retrying strategy.
            ServerVersion serverVersion = null;
#pragma warning disable EF1001
            var retryPolicy = Policy.Handle<Exception>(exception => MySqlTransientExceptionDetector.ShouldRetryOn(exception))
#pragma warning restore EF1001
                .WaitAndRetry(3, (count, context) => TimeSpan.FromMilliseconds(count * 250));

            serverVersion = retryPolicy.Execute(() => serverVersion = ServerVersion.AutoDetect(connectionString));

            return serverVersion;
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

        public class NpgsqlCaseInsensitiveSqlGenerationHelper : NpgsqlSqlGenerationHelper
        {
            const string EFMigrationsHisory = "__EFMigrationsHistory";
            public NpgsqlCaseInsensitiveSqlGenerationHelper(RelationalSqlGenerationHelperDependencies dependencies)
                : base(dependencies) { }
            public override string DelimitIdentifier(string identifier) =>
                base.DelimitIdentifier(identifier == EFMigrationsHisory ? identifier : identifier.ToLower());
            public override void DelimitIdentifier(StringBuilder builder, string identifier)
                => base.DelimitIdentifier(builder, identifier == EFMigrationsHisory ? identifier : identifier.ToLower());
        }
    }
}
