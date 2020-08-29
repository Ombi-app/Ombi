using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Ombi.Store.Migrations.OmbiMySql
{
    public partial class MobileDevices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
//            migrationBuilder.Sql(@"CREATE TABLE `MobileDevices` (
//    `Id` int NOT NULL AUTO_INCREMENT,
//    `Token` longtext CHARACTER SET utf8mb4 NULL,
//    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
//    `AddedAt` datetime(6) NOT NULL,
//    CONSTRAINT `PK_MobileDevices` PRIMARY KEY (`Id`),
//    CONSTRAINT `FK_MobileDevices_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT
//);");

            migrationBuilder.CreateTable(
                name: "MobileDevices",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false).Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Token = table.Column<string>(maxLength: 256, nullable: true),
                    UserId = table.Column<string>(maxLength: 255, nullable: false),
                    AddedAt = table.Column<DateTime>(maxLength: 256, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileDevices", x => x.Id);
                    table.ForeignKey(
                      name: "FK_MobileDevices_AspNetUsers_UserId",
                      column: x => x.UserId,
                      principalTable: "AspNetUsers",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Restrict);
                });


            migrationBuilder.CreateIndex(
                name: "IX_MobileDevices_UserId",
                table: "MobileDevices",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MobileDevices");

          
        }
    }
}
