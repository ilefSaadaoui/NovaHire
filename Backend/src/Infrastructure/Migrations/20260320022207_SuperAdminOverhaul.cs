using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SuperAdminOverhaul : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanAnnualPrice",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PlanCreatedAt",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PlanDescription",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PlanFeatures",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PlanIsActive",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PlanMaxJobOffers",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PlanMaxUsers",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PlanMonthlyPrice",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PlanName",
                table: "Companies");

            migrationBuilder.AlterColumn<Guid>(
                name: "PlanId",
                table: "Companies",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserEmail = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntityType = table.Column<string>(type: "text", nullable: true),
                    EntityId = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IPAddress = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    MonthlyPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AnnualPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxUsers = table.Column<int>(type: "integer", nullable: false),
                    MaxJobOffers = table.Column<int>(type: "integer", nullable: false),
                    Features = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SubscriptionPlans",
                columns: new[] { "Id", "AnnualPrice", "CreatedAt", "Description", "Features", "IsActive", "MaxJobOffers", "MaxUsers", "MonthlyPrice", "Name" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 0m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Plan gratuit pour découvrir la plateforme", "[\"Offres d'emploi basiques\", \"Candidatures illimitées\", \"Support email\"]", true, 3, 5, 0m, "Free" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 499m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Pour les petites entreprises", "[\"Tout du plan Free\", \"Branding personnalisé\", \"Analyses basiques\", \"Support prioritaire\"]", true, 10, 10, 49m, "Starter" },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), 999m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Pour les entreprises en croissance", "[\"Tout du plan Starter\", \"IA Scoring des candidats\", \"API Access\", \"Multi-utilisateurs avancé\", \"Support téléphonique\"]", true, 25, 25, 99m, "Professional" },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), 2999m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Solution sur mesure pour grandes entreprises", "[\"Tout du plan Professional\", \"SSO\", \"Audit logs\", \"SLA garantie\", \"Compte dédié\", \"Formation\"]", true, 100, 100, 299m, "Enterprise" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_PlanId",
                table: "Companies",
                column: "PlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_SubscriptionPlans_PlanId",
                table: "Companies",
                column: "PlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_SubscriptionPlans_PlanId",
                table: "Companies");

            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_Companies_PlanId",
                table: "Companies");

            migrationBuilder.AlterColumn<Guid>(
                name: "PlanId",
                table: "Companies",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlanAnnualPrice",
                table: "Companies",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlanCreatedAt",
                table: "Companies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PlanDescription",
                table: "Companies",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlanFeatures",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "PlanIsActive",
                table: "Companies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PlanMaxJobOffers",
                table: "Companies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlanMaxUsers",
                table: "Companies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PlanMonthlyPrice",
                table: "Companies",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PlanName",
                table: "Companies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "PlanAnnualPrice", "PlanCreatedAt", "PlanDescription", "PlanFeatures", "PlanIsActive", "PlanMaxJobOffers", "PlanMaxUsers", "PlanMonthlyPrice", "PlanName" },
                values: new object[] { 2999m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Solution sur mesure pour grandes entreprises", "[\"Tout du plan Professional\", \"SSO\", \"Audit logs\", \"SLA garantie\", \"Compte dédié\", \"Formation\"]", true, 100, 100, 299m, "Enterprise" });
        }
    }
}
