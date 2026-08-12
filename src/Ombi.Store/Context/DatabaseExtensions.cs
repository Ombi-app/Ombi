using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ombi.Store.Context
{
    public static class DatabaseExtensions
    {
        private static readonly HashSet<string> AllowedTables = new(StringComparer.Ordinal)
        {
            "SonarrCache",
            "SonarrEpisodeCache",
            "RadarrCache",
            "LidarrAlbumCache",
            "LidarrArtistCache",
        };

        /// <summary>
        /// Resets the auto-increment / identity counter for the given table back to the start.
        /// Cache sync jobs delete and re-insert rows frequently; without a reset the primary key
        /// grows indefinitely and can overflow Int32 (see #5224). Provider-specific:
        /// SQLite clears sqlite_sequence; MySQL resets AUTO_INCREMENT; PostgreSQL restarts the identity sequence.
        /// </summary>
        public static async Task ResetAutoIncrementAsync(this DatabaseFacade database, string tableName)
        {
            if (!AllowedTables.Contains(tableName))
            {
                throw new ArgumentException($"Auto-increment reset is not allowed for table '{tableName}'.", nameof(tableName));
            }

            var provider = database.ProviderName ?? string.Empty;

            if (provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                await database.ExecuteSqlRawAsync("DELETE FROM sqlite_sequence WHERE name = {0}", tableName);
            }
            else if (provider.Contains("MySql", StringComparison.OrdinalIgnoreCase)
                     || provider.Contains("MariaDb", StringComparison.OrdinalIgnoreCase))
            {
                await database.ExecuteSqlRawAsync($"ALTER TABLE `{tableName}` AUTO_INCREMENT = 1");
            }
            else if (provider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase)
                     || provider.Contains("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                await database.ExecuteSqlRawAsync($"ALTER SEQUENCE \"{tableName}_Id_seq\" RESTART WITH 1");
            }
        }
    }
}
