using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Ombi.Store.Migrations
{
    public partial class IssueComments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ResovledDate",
                table: "TvIssues",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TvIssues",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResovledDate",
                table: "MovieIssues",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "MovieIssues",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "IssueComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Comment = table.Column<string>(type: "TEXT", nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MovieIssueId = table.Column<int>(type: "INTEGER", nullable: true),
                    TvIssueId = table.Column<int>(type: "INTEGER", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueComments_MovieIssues_MovieIssueId",
                        column: x => x.MovieIssueId,
                        principalTable: "MovieIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IssueComments_TvIssues_TvIssueId",
                        column: x => x.TvIssueId,
                        principalTable: "TvIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IssueComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssueComments_MovieIssueId",
                table: "IssueComments",
                column: "MovieIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueComments_TvIssueId",
                table: "IssueComments",
                column: "TvIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueComments_UserId",
                table: "IssueComments",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssueComments");

            migrationBuilder.DropColumn(
                name: "ResovledDate",
                table: "TvIssues");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TvIssues");

            migrationBuilder.DropColumn(
                name: "ResovledDate",
                table: "MovieIssues");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MovieIssues");
        }
    }
}
