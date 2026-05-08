using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandJobOfferSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Deadline",
                table: "JobOffers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "JobOffers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExperienceLevel",
                table: "JobOffers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RemotePolicy",
                table: "JobOffers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "Skills",
                table: "JobOffers",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "JobOffers");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "JobOffers");

            migrationBuilder.DropColumn(
                name: "ExperienceLevel",
                table: "JobOffers");

            migrationBuilder.DropColumn(
                name: "RemotePolicy",
                table: "JobOffers");

            migrationBuilder.DropColumn(
                name: "Skills",
                table: "JobOffers");
        }
    }
}
