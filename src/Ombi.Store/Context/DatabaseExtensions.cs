using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ombi.Store.Context
{
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Resets the auto-increment/identity counter for the given table back to the start.
        /// This is used after clearing a cache table to prevent the primary key from
        /// growing indefinitely and eventually overflowing Int32 (see #5224).
        /// The statement issued is database provider specific, so we branch on the
        /// configured provider rather than assuming SQLite (see #5435).
        /// </summary>
        public static async Task ResetAutoIncrementAsync(this DatabaseFacade database, string tableName, string idColumn = "Id")
        {
            var provider = database.ProviderName ?? string.Empty;

            if (provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                await database.ExecuteSqlRawAsync("DELETE FROM sqlite_sequence WHERE name = {0}", tableName);
            }
            else if (provider.Contains("MySql", StringComparison.OrdinalIgnoreCase))
            {
                await database.ExecuteSqlRawAsync($"ALTER TABLE `{tableName}` AUTO_INCREMENT = 1");
            }
            else if (provider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
            {
                // Identifiers are folded to lower case by the Postgres configuration, so leaving
                // them unquoted lets Postgres resolve them correctly.
                await database.ExecuteSqlRawAsync($"ALTER TABLE {tableName} ALTER COLUMN {idColumn} RESTART WITH 1");
            }
        }
    }
}
