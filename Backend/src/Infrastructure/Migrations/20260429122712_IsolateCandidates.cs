using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IsolateCandidates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Candidates_Email",
                table: "Candidates");

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "Candidates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Mettre à jour les candidats existants avec un CompanyId valide
            migrationBuilder.Sql("UPDATE \"Candidates\" SET \"CompanyId\" = (SELECT \"Id\" FROM \"Companies\" LIMIT 1) WHERE EXISTS (SELECT 1 FROM \"Companies\");");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d001d001-d001-d001-d001-d001d001d001"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 29, 12, 27, 11, 745, DateTimeKind.Utc).AddTicks(806));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d002d002-d002-d002-d002-d002d002d002"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 29, 12, 27, 11, 745, DateTimeKind.Utc).AddTicks(1646));

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_CompanyId",
                table: "Candidates",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_Email_CompanyId",
                table: "Candidates",
                columns: new[] { "Email", "CompanyId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_Companies_CompanyId",
                table: "Candidates",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_Companies_CompanyId",
                table: "Candidates");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_CompanyId",
                table: "Candidates");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_Email_CompanyId",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Candidates");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d001d001-d001-d001-d001-d001d001d001"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 27, 21, 8, 14, 654, DateTimeKind.Utc).AddTicks(7757));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: new Guid("d002d002-d002-d002-d002-d002d002d002"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 27, 21, 8, 14, 654, DateTimeKind.Utc).AddTicks(8533));

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_Email",
                table: "Candidates",
                column: "Email",
                unique: true);
        }
    }
}
