using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Ombi.Helpers;
using System;

namespace Ombi.Store
{
    internal static class MigrationHelper
    {
        public static void InsertRole(this MigrationBuilder mb, string role)
        {
            mb.Sql($@"
INSERT INTO AspnetRoles(Id, ConcurrencyStamp, Name, NormalizedName) 
SELECT '{Guid.NewGuid()}','{Guid.NewGuid()}','{role}', '{role.ToUpper()}' 
WHERE NOT EXISTS(SELECT 1 FROM AspnetRoles WHERE Name = '{role}');");
        }

        public static void InsertRoleMySql(this MigrationBuilder mb, string role)
        {
            mb.Sql($@"
INSERT INTO AspNetRoles(Id, ConcurrencyStamp, Name, NormalizedName) 
SELECT '{Guid.NewGuid()}','{Guid.NewGuid()}','{role}', '{role.ToUpper()}' 
WHERE NOT EXISTS(SELECT 1 FROM AspNetRoles WHERE Name = '{role}');");
        }

        public static void InsertRolePostgres(this MigrationBuilder mb, string role)
        {
            mb.Sql($@"
INSERT INTO public.""AspNetRoles""(""Id"", ""ConcurrencyStamp"", ""Name"", ""NormalizedName"")
SELECT '{Guid.NewGuid()}','{Guid.NewGuid()}','{role}', '{role.ToUpper()}'
WHERE NOT EXISTS(SELECT 1 FROM public.""AspNetRoles"" WHERE ""Name"" = '{role}');");
        }

        public static void AddUserSelectableQualityProfiles(this MigrationBuilder mb) =>
            AddUserSelectableQualityProfiles(mb, DatabaseProvider.Sqlite);

        public static void AddUserSelectableQualityProfilesMySql(this MigrationBuilder mb) =>
            AddUserSelectableQualityProfiles(mb, DatabaseProvider.MySql);

        public static void AddUserSelectableQualityProfilesPostgres(this MigrationBuilder mb) =>
            AddUserSelectableQualityProfiles(mb, DatabaseProvider.Postgres);

        public static void RemoveUserSelectableQualityProfiles(this MigrationBuilder mb)
        {
            mb.DropTable("UserSelectableQualityProfile");
            mb.DropColumn("QualityOverride4K", "MovieRequests");
        }

        private static void AddUserSelectableQualityProfiles(MigrationBuilder mb, DatabaseProvider provider)
        {
            InsertQualityProfileRoles(mb, provider);

            var (integerType, userIdType, booleanType) = provider switch
            {
                DatabaseProvider.Sqlite => ("INTEGER", "TEXT", "INTEGER"),
                DatabaseProvider.MySql => ("int", "varchar(128)", "tinyint(1)"),
                DatabaseProvider.Postgres => ("integer", "character varying(128)", "boolean"),
                _ => throw new ArgumentOutOfRangeException(nameof(provider))
            };

            var tableBuilder = mb.CreateTable(
                name: "UserSelectableQualityProfile",
                columns: table => new UserSelectableQualityProfileColumns(
                    AddIdentityAnnotation(table.Column<int>(type: integerType, nullable: false), provider),
                    AddUserIdAnnotation(table.Column<string>(type: userIdType, maxLength: 128, nullable: false), provider),
                    table.Column<int>(type: integerType, nullable: false),
                    table.Column<int>(type: integerType, nullable: false),
                    table.Column<bool>(type: booleanType, nullable: false)),
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSelectableQualityProfile", x => x.Id);
                    table.ForeignKey("FK_UserSelectableQualityProfile_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
                });

            if (provider == DatabaseProvider.MySql)
            {
                tableBuilder.Annotation("MySql:CharSet", "utf8mb4");
            }

            mb.CreateIndex("IX_UserSelectableQualityProfile_UserId_Application_QualityProfileId_Is4K", "UserSelectableQualityProfile", new[] { "UserId", "Application", "QualityProfileId", "Is4K" }, unique: true);
            mb.AddColumn<int>("QualityOverride4K", "MovieRequests", type: integerType, nullable: false, defaultValue: 0);
        }

        private static void InsertQualityProfileRoles(MigrationBuilder mb, DatabaseProvider provider)
        {
            Action<string> insertRole = provider switch
            {
                DatabaseProvider.Sqlite => mb.InsertRole,
                DatabaseProvider.MySql => mb.InsertRoleMySql,
                DatabaseProvider.Postgres => mb.InsertRolePostgres,
                _ => throw new ArgumentOutOfRangeException(nameof(provider))
            };

            insertRole(OmbiRoles.SelectRadarrQualityProfile);
            insertRole(OmbiRoles.SelectSonarrQualityProfile);
        }

        private static OperationBuilder<AddColumnOperation> AddIdentityAnnotation(
            OperationBuilder<AddColumnOperation> column,
            DatabaseProvider provider) => provider switch
            {
                DatabaseProvider.Sqlite => column.Annotation("Sqlite:Autoincrement", true),
                DatabaseProvider.MySql => column.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                DatabaseProvider.Postgres => column.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                _ => throw new ArgumentOutOfRangeException(nameof(provider))
            };

        private static OperationBuilder<AddColumnOperation> AddUserIdAnnotation(
            OperationBuilder<AddColumnOperation> column,
            DatabaseProvider provider) => provider == DatabaseProvider.MySql
                ? column.Annotation("MySql:CharSet", "utf8mb4")
                : column;

        private enum DatabaseProvider
        {
            Sqlite,
            MySql,
            Postgres
        }

        private sealed class UserSelectableQualityProfileColumns
        {
            public UserSelectableQualityProfileColumns(
                OperationBuilder<AddColumnOperation> id,
                OperationBuilder<AddColumnOperation> userId,
                OperationBuilder<AddColumnOperation> application,
                OperationBuilder<AddColumnOperation> qualityProfileId,
                OperationBuilder<AddColumnOperation> is4K)
            {
                Id = id;
                UserId = userId;
                Application = application;
                QualityProfileId = qualityProfileId;
                Is4K = is4K;
            }

            public OperationBuilder<AddColumnOperation> Id { get; }
            public OperationBuilder<AddColumnOperation> UserId { get; }
            public OperationBuilder<AddColumnOperation> Application { get; }
            public OperationBuilder<AddColumnOperation> QualityProfileId { get; }
            public OperationBuilder<AddColumnOperation> Is4K { get; }
        }
    }
}
