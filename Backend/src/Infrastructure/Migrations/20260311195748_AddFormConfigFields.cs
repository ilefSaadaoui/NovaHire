using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFormConfigFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FormConfig_RequireEmail",
                table: "JobOffers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FormConfig_RequireFullName",
                table: "JobOffers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FormConfig_RequirePhone",
                table: "JobOffers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormConfig_RequireEmail",
                table: "JobOffers");

            migrationBuilder.DropColumn(
                name: "FormConfig_RequireFullName",
                table: "JobOffers");

            migrationBuilder.DropColumn(
                name: "FormConfig_RequirePhone",
                table: "JobOffers");
        }
    }
}
