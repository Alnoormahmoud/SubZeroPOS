using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubZeroPOS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWeeklyBackupSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastWeeklyBackup",
                table: "RestaurantSettings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WeeklyBackupEnabled",
                table: "RestaurantSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastWeeklyBackup",
                table: "RestaurantSettings");

            migrationBuilder.DropColumn(
                name: "WeeklyBackupEnabled",
                table: "RestaurantSettings");
        }
    }
}
