using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations
{
    public partial class IssueCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IssueCategory_temp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    SubjectPlaceholder = table.Column<string>(type: "TEXT", nullable: false),
                    DescriptionPlaceholder = table.Column<string>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueCategory", x => x.Id);
                });

            migrationBuilder.Sql("INSERT INTO IssueCategory_temp SELECT Id, Value, '', '' FROM IssueCategory;");
            migrationBuilder.Sql("PRAGMA foreign_keys=\"0\"", true);
            migrationBuilder.Sql("DROP TABLE IssueCategory", true);
            migrationBuilder.Sql("ALTER TABLE IssueCategory_temp RENAME TO IssueCategory", true);
            migrationBuilder.Sql("PRAGMA foreign_keys=\"1\"", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IssueCategory_temp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueCategory", x => x.Id);
                });

            migrationBuilder.Sql("INSERT INTO IssueCategory_temp SELECT Id, Name FROM IssueCategory;");
            migrationBuilder.Sql("PRAGMA foreign_keys=\"0\"", true);
            migrationBuilder.Sql("DROP TABLE IssueCategory", true);
            migrationBuilder.Sql("ALTER TABLE IssueCategory_temp RENAME TO IssueCategory", true);
            migrationBuilder.Sql("PRAGMA foreign_keys=\"1\"", true);
        }
    }
}