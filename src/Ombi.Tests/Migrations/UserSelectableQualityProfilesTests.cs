using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using NUnit.Framework;
using Ombi.Helpers;
using Ombi.Store.Context.Sqlite;
using MySqlMigration = Ombi.Store.Migrations.OmbiMySql.UserSelectableQualityProfiles;
using PostgresMigration = Ombi.Store.Migrations.OmbiPostgres.UserSelectableQualityProfiles;
using SqliteMigration = Ombi.Store.Migrations.OmbiSqlite.UserSelectableQualityProfiles;

namespace Ombi.Tests.Migrations
{
    [TestFixture]
    [NonParallelizable]
    public class UserSelectableQualityProfilesTests
    {
        private const string MigrationId = "20260723000000_UserSelectableQualityProfiles";
        private const string PreviousMigrationId = "20260417000001_AddPlexWatchlistUserStatus";
        private const string IndexName = "IX_UserSelectableQualityProfile_User_Application_Profile_Is4K";

        [Test]
        public async Task SqliteMigration_CreatesProfilesSchemaAndRolesForFreshAndExistingInstall()
        {
            var databasePath = System.IO.Path.GetTempFileName();
            try
            {
                var options = new DbContextOptionsBuilder<OmbiSqliteContext>()
                    .UseSqlite($"Data Source={databasePath}")
                    .Options;
                await using var context = new OmbiSqliteContext(options);
                await context.Database.MigrateAsync();

                Assert.Multiple(() =>
                {
                    Assert.That(context.Database.GetAppliedMigrations(), Does.Contain(MigrationId));
                    Assert.That(context.Roles.Any(x => x.Name == OmbiRoles.SelectRadarrQualityProfile), Is.True);
                    Assert.That(context.Roles.Any(x => x.Name == OmbiRoles.SelectSonarrQualityProfile), Is.True);
                    Assert.That(context.UserSelectableQualityProfiles.Count(), Is.Zero);
                    Assert.That(context.MovieRequests.All(x => x.QualityOverride4K == 0), Is.True);
                });

                var user = new Ombi.Store.Entities.OmbiUser
                {
                    Id = "allowlist-user",
                    UserName = "allowlist-user",
                    NormalizedUserName = "ALLOWLIST-USER",
                    StreamingCountry = "US"
                };
                context.Users.Add(user);
                context.UserSelectableQualityProfiles.Add(new Ombi.Store.Entities.UserSelectableQualityProfile
                {
                    UserId = user.Id,
                    Application = Ombi.Store.Entities.SelectableQualityProfileApplication.Radarr,
                    QualityProfileId = 7
                });
                await context.SaveChangesAsync();
                context.UserSelectableQualityProfiles.Add(new Ombi.Store.Entities.UserSelectableQualityProfile
                {
                    UserId = user.Id,
                    Application = Ombi.Store.Entities.SelectableQualityProfileApplication.Radarr,
                    QualityProfileId = 7
                });
                Assert.ThrowsAsync<DbUpdateException>(async () => await context.SaveChangesAsync());
                context.ChangeTracker.Clear();
                context.Users.Remove(await context.Users.FindAsync(user.Id));
                await context.SaveChangesAsync();
                Assert.That(context.UserSelectableQualityProfiles.Count(), Is.Zero);

                var migrator = context.Database.GetService<IMigrator>();
                await migrator.MigrateAsync(PreviousMigrationId);

                Assert.Multiple(() =>
                {
                    Assert.That(context.Database.GetAppliedMigrations(), Does.Not.Contain(MigrationId));
                    Assert.That(context.Roles.Any(x => x.Name == OmbiRoles.SelectRadarrQualityProfile), Is.False);
                    Assert.That(context.Roles.Any(x => x.Name == OmbiRoles.SelectSonarrQualityProfile), Is.False);
                });

                await context.Database.MigrateAsync();

                Assert.Multiple(() =>
                {
                    Assert.That(context.Database.GetAppliedMigrations(), Does.Contain(MigrationId));
                    Assert.That(context.Roles.Count(x => x.Name == OmbiRoles.SelectRadarrQualityProfile), Is.EqualTo(1));
                    Assert.That(context.Roles.Count(x => x.Name == OmbiRoles.SelectSonarrQualityProfile), Is.EqualTo(1));
                    Assert.That(context.UserSelectableQualityProfiles.Count(), Is.Zero);
                    Assert.That(context.MovieRequests.All(x => x.QualityOverride4K == 0), Is.True);
                });
            }
            finally
            {
                System.IO.File.Delete(databasePath);
            }
        }

        [TestCase("Sqlite", "DELETE FROM AspnetRoles WHERE Name IN ('SelectRadarrQualityProfile', 'SelectSonarrQualityProfile');")]
        [TestCase("MySql", "DELETE FROM AspNetRoles WHERE Name IN ('SelectRadarrQualityProfile', 'SelectSonarrQualityProfile');")]
        [TestCase("Postgres", "DELETE FROM public.\"AspNetRoles\" WHERE \"Name\" IN ('SelectRadarrQualityProfile', 'SelectSonarrQualityProfile');")]
        public void MigrationOperations_UseProviderSpecificRollbackSqlAndSafeIndexName(string provider, string expectedSql)
        {
            Migration migration = provider switch
            {
                "Sqlite" => new SqliteMigration(),
                "MySql" => new MySqlMigration(),
                "Postgres" => new PostgresMigration(),
                _ => throw new System.ArgumentOutOfRangeException(nameof(provider))
            };

            var index = migration.UpOperations.OfType<CreateIndexOperation>().Single();
            var downOperations = migration.DownOperations;

            Assert.Multiple(() =>
            {
                Assert.That(index.Name, Is.EqualTo(IndexName));
                Assert.That(System.Text.Encoding.ASCII.GetByteCount(index.Name), Is.LessThanOrEqualTo(63));
                Assert.That(index.Table, Is.EqualTo("UserSelectableQualityProfile"));
                Assert.That(index.Columns, Is.EqualTo(new[] { "UserId", "Application", "QualityProfileId", "Is4K" }));
                Assert.That(index.IsUnique, Is.True);
                Assert.That(downOperations, Has.Count.EqualTo(3));
                Assert.That(((SqlOperation)downOperations[0]).Sql, Is.EqualTo(expectedSql));
                Assert.That(((DropTableOperation)downOperations[1]).Name, Is.EqualTo("UserSelectableQualityProfile"));
                Assert.That(((DropColumnOperation)downOperations[2]).Name, Is.EqualTo("QualityOverride4K"));
            });
        }
    }
}
