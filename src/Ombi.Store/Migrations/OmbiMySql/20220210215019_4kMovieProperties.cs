using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ombi.Store.Migrations.OmbiMySql
{
    public partial class _4kMovieProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Approved4K",
                table: "MovieRequests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Available4K",
                table: "MovieRequests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Denied4K",
                table: "MovieRequests",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeniedReason4K",
                table: "MovieRequests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "MarkedAsApproved4K",
                table: "MovieRequests",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "MarkedAsAvailable4K",
                table: "MovieRequests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MarkedAsDenied4K",
                table: "MovieRequests",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedDate4k",
                table: "MovieRequests",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved4K",
                table: "MovieRequests");

            migrationBuilder.DropColumn(
                name: "Available4K",
                table: "MovieRequests");

            migrationBuilder.DropColumn(
                name: "Denied4K",
                table: "MovieRequests");

            migrationBuilder.DropColumn(
                name: "DeniedReason4K",
                table: "MovieRequests");

            migrationBuilder.DropColumn(
                name: "MarkedAsApproved4K",
                table: "MovieRequests");

            migrationBuilder.DropColumn(
                name: "MarkedAsAvailable4K",
                table: "MovieRequests");

            migrationBuilder.DropColumn(
                name: "MarkedAsDenied4K",
                table: "MovieRequests");

            migrationBuilder.DropColumn(
                name: "RequestedDate4k",
                table: "MovieRequests");
        }
    }
}
