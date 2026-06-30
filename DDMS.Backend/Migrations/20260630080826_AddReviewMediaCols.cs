using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewMediaCols : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "image_urls",
                table: "reviews",
                type: "json",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "video_urls",
                table: "reviews",
                type: "json",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "created_at",
                value: new DateTime(2026, 6, 30, 8, 8, 25, 571, DateTimeKind.Utc).AddTicks(8798));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "created_at",
                value: new DateTime(2026, 6, 30, 8, 8, 25, 572, DateTimeKind.Utc).AddTicks(24));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "created_at",
                value: new DateTime(2026, 6, 30, 8, 8, 25, 572, DateTimeKind.Utc).AddTicks(30));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "created_at",
                value: new DateTime(2026, 6, 30, 8, 8, 25, 572, DateTimeKind.Utc).AddTicks(31));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_urls",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "video_urls",
                table: "reviews");

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "created_at",
                value: new DateTime(2026, 6, 29, 8, 12, 3, 208, DateTimeKind.Utc).AddTicks(9291));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "created_at",
                value: new DateTime(2026, 6, 29, 8, 12, 3, 209, DateTimeKind.Utc).AddTicks(472));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "created_at",
                value: new DateTime(2026, 6, 29, 8, 12, 3, 209, DateTimeKind.Utc).AddTicks(478));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "created_at",
                value: new DateTime(2026, 6, 29, 8, 12, 3, 209, DateTimeKind.Utc).AddTicks(479));
        }
    }
}
