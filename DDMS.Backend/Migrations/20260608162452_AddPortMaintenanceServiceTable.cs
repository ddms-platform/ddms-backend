using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPortMaintenanceServiceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "port_maintenance_service",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icon_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    price = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    description = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", maxLength: 6, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.InsertData(
                table: "port_maintenance_service",
                columns: new[] { "id", "created_at", "description", "icon_code", "name", "price" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 6, 8, 16, 24, 51, 374, DateTimeKind.Utc).AddTicks(8111), null, "Settings", "Bảo trì định kỳ", 1200000m },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 6, 8, 16, 24, 51, 374, DateTimeKind.Utc).AddTicks(9171), null, "AlertTriangle", "Sửa chữa khẩn cấp", null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 6, 8, 16, 24, 51, 374, DateTimeKind.Utc).AddTicks(9175), null, "User", "Vệ sinh thân tàu", 500000m },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 6, 8, 16, 24, 51, 374, DateTimeKind.Utc).AddTicks(9177), null, "Zap", "Kiểm tra hệ thống điện", 300000m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "port_maintenance_service");
        }
    }
}
