using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "QuizExpiresAt",
                table: "JobApplications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuizScore",
                table: "JobApplications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "QuizSent",
                table: "JobApplications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d001d001-d001-d001-d001-d001d001d001"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 5, 23, 56, 37, 573, DateTimeKind.Utc).AddTicks(3421));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d002d002-d002-d002-d002-d002d002d002"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 5, 23, 56, 37, 573, DateTimeKind.Utc).AddTicks(4479));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuizExpiresAt",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "QuizScore",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "QuizSent",
                table: "JobApplications");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d001d001-d001-d001-d001-d001d001d001"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 5, 23, 39, 12, 110, DateTimeKind.Utc).AddTicks(2228));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d002d002-d002-d002-d002-d002d002d002"),
                column: "CreatedAt",
                value: new DateTime(2026, 5, 5, 23, 39, 12, 110, DateTimeKind.Utc).AddTicks(2996));
        }
    }
}
