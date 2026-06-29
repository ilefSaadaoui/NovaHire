using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSuperAdminEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d001d001-d001-d001-d001-d001d001d001"),
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 22, 44, 10, 183, DateTimeKind.Utc).AddTicks(1445));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d002d002-d002-d002-d002-d002d002d002"),
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 22, 44, 10, 183, DateTimeKind.Utc).AddTicks(2599));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d001d001-d001-d001-d001-d001d001d001"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 6, 0, 20, 7, 637, DateTimeKind.Utc).AddTicks(788));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d002d002-d002-d002-d002-d002d002d002"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 6, 0, 20, 7, 637, DateTimeKind.Utc).AddTicks(2477));
        }
    }
}
