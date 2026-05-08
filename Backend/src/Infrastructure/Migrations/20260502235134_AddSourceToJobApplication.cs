using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceToJobApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "JobApplications",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d001d001-d001-d001-d001-d001d001d001"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 2, 23, 51, 33, 595, DateTimeKind.Utc).AddTicks(9122));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d002d002-d002-d002-d002-d002d002d002"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 2, 23, 51, 33, 595, DateTimeKind.Utc).AddTicks(9882));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "JobApplications");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d001d001-d001-d001-d001-d001d001d001"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 2, 21, 44, 48, 835, DateTimeKind.Utc).AddTicks(2655));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d002d002-d002-d002-d002-d002d002d002"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 2, 21, 44, 48, 835, DateTimeKind.Utc).AddTicks(3495));
        }
    }
}
