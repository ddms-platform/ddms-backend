using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceServiceAndStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "port_maintenance_service_id",
                table: "boat_maintenances",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "boat_maintenances",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "'pending'",
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "created_at",
                value: new DateTime(2026, 6, 10, 13, 39, 25, 449, DateTimeKind.Utc).AddTicks(6083));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "created_at",
                value: new DateTime(2026, 6, 10, 13, 39, 25, 449, DateTimeKind.Utc).AddTicks(7125));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "created_at",
                value: new DateTime(2026, 6, 10, 13, 39, 25, 449, DateTimeKind.Utc).AddTicks(7130));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "created_at",
                value: new DateTime(2026, 6, 10, 13, 39, 25, 449, DateTimeKind.Utc).AddTicks(7131));

            migrationBuilder.CreateIndex(
                name: "IX_boat_maintenances_port_maintenance_service_id",
                table: "boat_maintenances",
                column: "port_maintenance_service_id");

            migrationBuilder.AddForeignKey(
                name: "fk_maintenance_port_service",
                table: "boat_maintenances",
                column: "port_maintenance_service_id",
                principalTable: "port_maintenance_service",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_maintenance_port_service",
                table: "boat_maintenances");

            migrationBuilder.DropIndex(
                name: "IX_boat_maintenances_port_maintenance_service_id",
                table: "boat_maintenances");

            migrationBuilder.DropColumn(
                name: "port_maintenance_service_id",
                table: "boat_maintenances");

            migrationBuilder.DropColumn(
                name: "status",
                table: "boat_maintenances");

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "created_at",
                value: new DateTime(2026, 6, 8, 16, 24, 51, 374, DateTimeKind.Utc).AddTicks(8111));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "created_at",
                value: new DateTime(2026, 6, 8, 16, 24, 51, 374, DateTimeKind.Utc).AddTicks(9171));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "created_at",
                value: new DateTime(2026, 6, 8, 16, 24, 51, 374, DateTimeKind.Utc).AddTicks(9175));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "created_at",
                value: new DateTime(2026, 6, 8, 16, 24, 51, 374, DateTimeKind.Utc).AddTicks(9177));
        }
    }
}
