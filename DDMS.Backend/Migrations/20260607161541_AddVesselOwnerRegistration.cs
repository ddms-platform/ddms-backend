using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddVesselOwnerRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "owner_profiles",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValueSql: "'Pending'",
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "beam",
                table: "boats",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "document_url",
                table: "boats",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "expected_docking_date",
                table: "boats",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "length",
                table: "boats",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "mooring_type",
                table: "boats",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "registration_number",
                table: "boats",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "required_services",
                table: "boats",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "owner_profiles");

            migrationBuilder.DropColumn(
                name: "beam",
                table: "boats");

            migrationBuilder.DropColumn(
                name: "document_url",
                table: "boats");

            migrationBuilder.DropColumn(
                name: "expected_docking_date",
                table: "boats");

            migrationBuilder.DropColumn(
                name: "length",
                table: "boats");

            migrationBuilder.DropColumn(
                name: "mooring_type",
                table: "boats");

            migrationBuilder.DropColumn(
                name: "registration_number",
                table: "boats");

            migrationBuilder.DropColumn(
                name: "required_services",
                table: "boats");
        }
    }
}
