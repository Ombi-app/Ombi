using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ombi.Store.Migrations.OmbiMySql
{
    public partial class MobileDevices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE TABLE `MobileDevices` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Token` longtext CHARACTER SET utf8mb4 NULL,
    `UserId` varchar(255) COLLATE utf8mb4_bin NOT NULL,
    `AddedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_MobileDevices` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_MobileDevices_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT
);");
            

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
