using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewQuestionsToJobApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AIAnalysis_InterviewQuestions",
                table: "JobApplications",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d001d001-d001-d001-d001-d001d001d001"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 23, 10, 18, 0, 995, DateTimeKind.Utc).AddTicks(8889));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d002d002-d002-d002-d002-d002d002d002"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 23, 10, 18, 0, 996, DateTimeKind.Utc).AddTicks(147));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AIAnalysis_InterviewQuestions",
                table: "JobApplications");

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
    }
}
