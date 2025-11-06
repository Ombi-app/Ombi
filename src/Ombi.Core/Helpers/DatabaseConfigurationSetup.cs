using System;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Ombi.Core.Models;
using Polly;
using Pomelo.EntityFrameworkCore.MySql.Storage.Internal;

namespace Ombi.Core.Helpers;

public static class DatabaseConfigurationSetup
{
    public static void ConfigurePostgres(DbContextOptionsBuilder options, PerDatabaseConfiguration config)
    {
        options.UseNpgsql(config.ConnectionString, b =>
        {
            b.EnableRetryOnFailure();
        }).ReplaceService<ISqlGenerationHelper, NpgsqlCaseInsensitiveSqlGenerationHelper>();
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

    public static void ConfigureSqlite(DbContextOptionsBuilder options, PerDatabaseConfiguration config)
    {
        if (string.IsNullOrEmpty(config.ConnectionString))
        {
            throw new ArgumentNullException("ConnectionString for the SQLite database is empty");
        }

        options.UseSqlite(config.ConnectionString, b =>
        {
            b.ExecutionStrategy(dependencies => new SqliteRetryExecutionStrategy(dependencies));
        });
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

    public class SqliteRetryExecutionStrategy : ExecutionStrategy
    {
        private new const int MaxRetryCount = 10;
        private static new readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(30);

        public SqliteRetryExecutionStrategy(ExecutionStrategyDependencies dependencies)
            : base(dependencies, MaxRetryCount, MaxRetryDelay)
        {
        }

        protected override bool ShouldRetryOn(Exception exception)
        {
            // Retry on SQLite "database is locked" errors (error code 5)
            // Also retry on "database is busy" errors (error code 6)
            if (exception is SqliteException sqliteException)
            {
                return sqliteException.SqliteErrorCode == 5 || // SQLITE_BUSY
                       sqliteException.SqliteErrorCode == 6;   // SQLITE_LOCKED
            }

            // Also check for DbUpdateException wrapping SqliteException
            if (exception is Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException &&
                dbUpdateException.InnerException is SqliteException innerSqliteException)
            {
                return innerSqliteException.SqliteErrorCode == 5 ||
                       innerSqliteException.SqliteErrorCode == 6;
            }

            return false;
        }

        protected override TimeSpan? GetNextDelay(Exception lastException)
        {
            // Use exponential backoff with jitter
            var baseDelay = base.GetNextDelay(lastException);
            if (baseDelay == null)
            {
                return null;
            }

            // Add some randomness to prevent thundering herd
            var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100));
            return baseDelay.Value + jitter;
        }
    }
}