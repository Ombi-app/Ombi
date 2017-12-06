using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Ombi.Store.Migrations
{
    public partial class Issues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
            
            PRAGMA foreign_keys=OFF;

            CREATE TABLE IssueCategory (
	Id	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	VALUE	TEXT);

DROP TABLE TvIssues;
DROP TABLE MovieIssues;

            CREATE TABLE TvIssues (
	Id	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	Description	TEXT,
	IssueId	INTEGER,
	Subect	TEXT,
	TvId	INTEGER NOT NULL,
    IssueCategoryId INTEGER NOT NULL,
    Status INTEGER NOT NULL,
    ResovledDate TEXT NULL,
	FOREIGN KEY(IssueId) REFERENCES ChildRequests(Id) ON DELETE RESTRICT,
	FOREIGN KEY(TvId) REFERENCES ChildRequests(Id) ON DELETE CASCADE,
	FOREIGN KEY(IssueCategoryId) REFERENCES IssueCategory(Id) ON DELETE CASCADE);

CREATE TABLE MovieIssues (
	Id	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	Description	TEXT,
	IssueId	INTEGER,
	MovieId	INTEGER NOT NULL,
	Subect	TEXT,
    IssueCategoryId INTEGER NOT NULL,
    Status INTEGER NOT NULL,
    ResovledDate TEXT NULL,
	FOREIGN KEY(IssueId) REFERENCES MovieRequests(Id) ON DELETE RESTRICT,
	FOREIGN KEY(MovieId) REFERENCES MovieRequests(Id) ON DELETE CASCADE,
	FOREIGN KEY(IssueCategoryId) REFERENCES IssueCategory(Id) ON DELETE CASCADE);


PRAGMA foreign_key_check;

            PRAGMA foreign_keys=ON;
");

            //migrationBuilder.AddColumn<int>(
            //    name: "IssueCategoryId",
            //    table: "TvIssues",
            //    type: "INTEGER",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "IssueCategoryId",
            //    table: "MovieIssues",
            //    type: "INTEGER",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.CreateTable(
            //    name: "IssueCategory",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        Value = table.Column<string>(type: "TEXT", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_IssueCategory", x => x.Id);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_TvIssues_IssueCategoryId",
            //    table: "TvIssues",
            //    column: "IssueCategoryId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_MovieIssues_IssueCategoryId",
            //    table: "MovieIssues",
            //    column: "IssueCategoryId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_MovieIssues_IssueCategory_IssueCategoryId",
            //    table: "MovieIssues",
            //    column: "IssueCategoryId",
            //    principalTable: "IssueCategory",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_TvIssues_IssueCategory_IssueCategoryId",
            //    table: "TvIssues",
            //    column: "IssueCategoryId",
            //    principalTable: "IssueCategory",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovieIssues_IssueCategory_IssueCategoryId",
                table: "MovieIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_TvIssues_IssueCategory_IssueCategoryId",
                table: "TvIssues");

            migrationBuilder.DropTable(
                name: "IssueCategory");

            migrationBuilder.DropIndex(
                name: "IX_TvIssues_IssueCategoryId",
                table: "TvIssues");

            migrationBuilder.DropIndex(
                name: "IX_MovieIssues_IssueCategoryId",
                table: "MovieIssues");

            migrationBuilder.DropColumn(
                name: "IssueCategoryId",
                table: "TvIssues");

            migrationBuilder.DropColumn(
                name: "IssueCategoryId",
                table: "MovieIssues");
        }
    }
}
