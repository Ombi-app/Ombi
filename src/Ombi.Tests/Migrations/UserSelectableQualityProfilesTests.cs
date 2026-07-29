using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NUnit.Framework;
using Ombi.Helpers;
using Ombi.Store.Context.Sqlite;

namespace Ombi.Tests.Migrations
{
    [TestFixture]
    [NonParallelizable]
    public class UserSelectableQualityProfilesTests
    {
        private const string MigrationId = "20260723000000_UserSelectableQualityProfiles";
        private const string PreviousMigrationId = "20260417000001_AddPlexWatchlistUserStatus";

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

                await context.Database.EnsureDeletedAsync();
                await context.Database.GetService<IMigrator>().MigrateAsync(PreviousMigrationId);
                await context.Database.MigrateAsync();

                Assert.Multiple(() =>
                {
                    Assert.That(context.Database.GetAppliedMigrations(), Does.Contain(MigrationId));
                    Assert.That(context.Roles.Any(x => x.Name == OmbiRoles.SelectRadarrQualityProfile), Is.True);
                    Assert.That(context.Roles.Any(x => x.Name == OmbiRoles.SelectSonarrQualityProfile), Is.True);
                    Assert.That(context.UserSelectableQualityProfiles.Count(), Is.Zero);
                    Assert.That(context.MovieRequests.All(x => x.QualityOverride4K == 0), Is.True);
                });
            }
            finally
            {
                System.IO.File.Delete(databasePath);
            }
        }
    }
}
