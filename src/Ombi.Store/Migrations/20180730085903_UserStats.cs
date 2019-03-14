using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations
{
    public partial class UserStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "MarkedAsApproved",
                table: "MovieRequests",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "MarkedAsAvailable",
                table: "MovieRequests",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MarkedAsDenied",
                table: "MovieRequests",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "MarkedAsApproved",
                table: "ChildRequests",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "MarkedAsAvailable",
                table: "ChildRequests",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MarkedAsDenied",
                table: "ChildRequests",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarkedAsApproved",
                table: "MovieRequests");

            migrationBuilder.DropColumn(
                name: "MarkedAsAvailable",
                table: "MovieRequests");

            migrationBuilder.DropColumn(
                name: "MarkedAsDenied",
                table: "MovieRequests");

            migrationBuilder.DropColumn(
                name: "MarkedAsApproved",
                table: "ChildRequests");

            migrationBuilder.DropColumn(
                name: "MarkedAsAvailable",
                table: "ChildRequests");

            migrationBuilder.DropColumn(
                name: "MarkedAsDenied",
                table: "ChildRequests");
        }
    }
}
