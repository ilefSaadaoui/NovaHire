using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ContactPhone = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    PrimaryColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#FFD700"),
                    SecondaryColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#000000"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PlanDescription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PlanMonthlyPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PlanAnnualPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PlanMaxUsers = table.Column<int>(type: "integer", nullable: false),
                    PlanMaxJobOffers = table.Column<int>(type: "integer", nullable: false),
                    PlanFeatures = table.Column<string>(type: "text", nullable: false),
                    PlanIsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PlanCreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubscriptionExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    EmailConfirmationToken = table.Column<string>(type: "text", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "text", nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "JobOffers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SalaryRange = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    FormConfig_RequireCV = table.Column<bool>(type: "boolean", nullable: false),
                    FormConfig_RequireCoverLetter = table.Column<bool>(type: "boolean", nullable: false),
                    FormConfig_RequirePortfolio = table.Column<bool>(type: "boolean", nullable: false),
                    FormConfig_RequireLinkedIn = table.Column<bool>(type: "boolean", nullable: false),
                    FormConfig_CustomFields = table.Column<string>(type: "text", nullable: false),
                    FormConfig_RequiredDocuments = table.Column<string>(type: "text", nullable: false),
                    DisplayConfig_ShowCompanyName = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayConfig_ShowCompanyLogo = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayConfig_ShowSalary = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayConfig_ShowLocation = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayConfig_CustomCSS = table.Column<string>(type: "text", nullable: true),
                    DisplayConfig_HeaderImageUrl = table.Column<string>(type: "text", nullable: true),
                    PublicUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobOffers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobOffers_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobOfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: true),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    CVUrl = table.Column<string>(type: "text", nullable: false),
                    CoverLetterUrl = table.Column<string>(type: "text", nullable: true),
                    PortfolioUrl = table.Column<string>(type: "text", nullable: true),
                    LinkedInUrl = table.Column<string>(type: "text", nullable: true),
                    CustomFieldsData = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AIAnalysis_OverallScore = table.Column<int>(type: "integer", nullable: true),
                    AIAnalysis_ExperienceScore = table.Column<int>(type: "integer", nullable: true),
                    AIAnalysis_EducationScore = table.Column<int>(type: "integer", nullable: true),
                    AIAnalysis_SkillsScore = table.Column<int>(type: "integer", nullable: true),
                    AIAnalysis_ExtractedData_WorkExperiences = table.Column<string>(type: "text", nullable: true),
                    AIAnalysis_ExtractedData_Educations = table.Column<string>(type: "text", nullable: true),
                    AIAnalysis_ExtractedData_Skills = table.Column<string>(type: "text", nullable: true),
                    AIAnalysis_ExtractedData_Languages = table.Column<List<string>>(type: "text[]", nullable: true),
                    AIAnalysis_ExtractedData_Certifications = table.Column<List<string>>(type: "text[]", nullable: true),
                    AIAnalysis_AutoGeneratedSummary = table.Column<string>(type: "text", nullable: true),
                    AIAnalysis_IdentifiedSkills = table.Column<string>(type: "text", nullable: true),
                    AIAnalysis_Strengths = table.Column<List<string>>(type: "text[]", nullable: true),
                    AIAnalysis_Weaknesses = table.Column<List<string>>(type: "text[]", nullable: true),
                    AIAnalysis_AIRecommendation = table.Column<string>(type: "text", nullable: true),
                    AIAnalysis_AnalyzedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecruiterNotes = table.Column<string>(type: "text", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobApplications_JobOffers_JobOfferId",
                        column: x => x.JobOfferId,
                        principalTable: "JobOffers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobApplications_Users_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_JobApplications_Users_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "Address", "City", "ContactEmail", "ContactPhone", "Country", "CreatedAt", "Description", "IsActive", "LogoUrl", "Name", "PostalCode", "PrimaryColor", "SecondaryColor", "SubscriptionExpiresAt", "UpdatedAt", "PlanAnnualPrice", "PlanCreatedAt", "PlanDescription", "PlanFeatures", "PlanId", "PlanIsActive", "PlanMaxJobOffers", "PlanMaxUsers", "PlanMonthlyPrice", "PlanName" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), null, null, "contact@neoledge.com", null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Société de démonstration pour la plateforme", true, null, "NeoLedge Demo Company", null, "#FFD700", "#000000", null, null, 2999m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Solution sur mesure pour grandes entreprises", "[\"Tout du plan Professional\", \"SSO\", \"Audit logs\", \"SLA garantie\", \"Compte dédié\", \"Formation\"]", new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), true, 100, 100, 299m, "Enterprise" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CompanyId", "CreatedAt", "Email", "EmailConfirmationToken", "EmailConfirmed", "FirstName", "IsActive", "LastLoginAt", "LastName", "PasswordHash", "PasswordResetToken", "PasswordResetTokenExpiry", "PhoneNumber", "RefreshToken", "RefreshTokenExpiry", "Role", "UpdatedAt" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@neoledge.com", null, true, "Admin", true, null, "NeoLedge", "6G94qKPK8LYNjnTllCqm2G3BUM08AzOK7yW30tfjrMc=", null, null, null, null, null, 0, null });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Name",
                table: "Companies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_CandidateId",
                table: "JobApplications",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_Email",
                table: "JobApplications",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobOfferId",
                table: "JobApplications",
                column: "JobOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_ReviewedById",
                table: "JobApplications",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_Status",
                table: "JobApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_SubmittedAt",
                table: "JobApplications",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JobOffers_CompanyId",
                table: "JobOffers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOffers_CreatedById",
                table: "JobOffers",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_JobOffers_PublishedAt",
                table: "JobOffers",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JobOffers_Status",
                table: "JobOffers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId",
                table: "Users",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobApplications");

            migrationBuilder.DropTable(
                name: "JobOffers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
