using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ombi.Store.Context
{
    /// <summary>
    /// Provider-aware helpers for external database maintenance operations.
    /// </summary>
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Resets the auto-increment / identity counter for the given table back to the start.
        /// Cache sync jobs delete and re-insert rows frequently; without a reset the primary key
        /// grows indefinitely and can overflow Int32 (see #5224). Provider-specific:
        /// SQLite clears sqlite_sequence; MySQL resets AUTO_INCREMENT; PostgreSQL restarts the identity sequence.
        /// <para>
        /// Call this <b>outside</b> an explicit transaction. MySQL/MariaDB <c>ALTER TABLE</c> causes an
        /// implicit commit, so running it inside a transaction can leave prior deletes non-rollbackable.
        /// </para>
        /// </summary>
        /// <param name="database">The EF Core database facade for the external context.</param>
        /// <param name="tableName">Name of a cache table whose identity counter should be reset.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tableName"/> is not an allowed cache table.</exception>
        public static async Task ResetAutoIncrementAsync(this DatabaseFacade database, string tableName)
        {
            var provider = database.ProviderName ?? string.Empty;

            if (provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                // Keep the same allowlist as MySQL/Postgres even though the name is parameterized.
                if (tableName is not ("SonarrCache" or "SonarrEpisodeCache" or "RadarrCache" or "LidarrAlbumCache" or "LidarrArtistCache"))
                {
                    throw new ArgumentException($"Auto-increment reset is not allowed for table '{tableName}'.", nameof(tableName));
                }

                await database.ExecuteSqlRawAsync("DELETE FROM sqlite_sequence WHERE name = {0}", tableName);
                return;
            }

            if (provider.Contains("MySql", StringComparison.OrdinalIgnoreCase)
                || provider.Contains("MariaDb", StringComparison.OrdinalIgnoreCase))
            {
                // Identifiers cannot be parameterized; use fixed statements per allowed table.
                var sql = tableName switch
                {
                    "SonarrCache" => "ALTER TABLE `SonarrCache` AUTO_INCREMENT = 1",
                    "SonarrEpisodeCache" => "ALTER TABLE `SonarrEpisodeCache` AUTO_INCREMENT = 1",
                    "RadarrCache" => "ALTER TABLE `RadarrCache` AUTO_INCREMENT = 1",
                    "LidarrAlbumCache" => "ALTER TABLE `LidarrAlbumCache` AUTO_INCREMENT = 1",
                    "LidarrArtistCache" => "ALTER TABLE `LidarrArtistCache` AUTO_INCREMENT = 1",
                    _ => throw new ArgumentException($"Auto-increment reset is not allowed for table '{tableName}'.", nameof(tableName)),
                };
                await database.ExecuteSqlRawAsync(sql);
                return;
            }

            if (provider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase)
                || provider.Contains("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                var sql = tableName switch
                {
                    "SonarrCache" => "ALTER SEQUENCE \"SonarrCache_Id_seq\" RESTART WITH 1",
                    "SonarrEpisodeCache" => "ALTER SEQUENCE \"SonarrEpisodeCache_Id_seq\" RESTART WITH 1",
                    "RadarrCache" => "ALTER SEQUENCE \"RadarrCache_Id_seq\" RESTART WITH 1",
                    "LidarrAlbumCache" => "ALTER SEQUENCE \"LidarrAlbumCache_Id_seq\" RESTART WITH 1",
                    "LidarrArtistCache" => "ALTER SEQUENCE \"LidarrArtistCache_Id_seq\" RESTART WITH 1",
                    _ => throw new ArgumentException($"Auto-increment reset is not allowed for table '{tableName}'.", nameof(tableName)),
                };
                await database.ExecuteSqlRawAsync(sql);
            }
        }
    }
}
