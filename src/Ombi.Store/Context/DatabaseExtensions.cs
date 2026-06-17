using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ombi.Store.Context
{
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Resets the auto-increment counter for the given table back to the start.
        /// This is only required on SQLite, where we clear the 'sqlite_sequence'
        /// table to prevent the primary key from growing indefinitely and eventually
        /// overflowing Int32 (see #5224). 'sqlite_sequence' does not exist on MySQL or
        /// PostgreSQL, so this is a no-op for those providers (see #5435).
        /// </summary>
        public static async Task ResetAutoIncrementAsync(this DatabaseFacade database, string tableName)
        {
            var provider = database.ProviderName ?? string.Empty;

            if (provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                await database.ExecuteSqlRawAsync("DELETE FROM sqlite_sequence WHERE name = {0}", tableName);
            }
        }
    }
}
