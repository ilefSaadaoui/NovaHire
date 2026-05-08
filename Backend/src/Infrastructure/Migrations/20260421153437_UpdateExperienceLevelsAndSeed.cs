using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExperienceLevelsAndSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d001d001-d001-d001-d001-d001d001d001"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 21, 15, 34, 36, 205, DateTimeKind.Utc).AddTicks(5143));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d002d002-d002-d002-d002-d002d002d002"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 21, 15, 34, 36, 205, DateTimeKind.Utc).AddTicks(5908));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d001d001-d001-d001-d001-d001d001d001"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 10, 57, 11, 523, DateTimeKind.Utc).AddTicks(2223));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d002d002-d002-d002-d002-d002d002d002"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 16, 10, 57, 11, 523, DateTimeKind.Utc).AddTicks(4364));
        }
    }
}
