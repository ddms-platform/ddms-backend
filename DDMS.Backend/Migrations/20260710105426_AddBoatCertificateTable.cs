using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddBoatCertificateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "compliance_status",
                table: "boats",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "'valid'",
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "boat_certificates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    boat_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    certificate_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    document_url = table.Column<string>(type: "text", nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    public_id = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    expiry_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValueSql: "'pending'", collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rejection_reason = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    verified_by = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    verified_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    reminder_sent_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", maxLength: 6, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", maxLength: 6, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_boat_certificates_boat",
                        column: x => x.boat_id,
                        principalTable: "boats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_boat_certificates_verifier",
                        column: x => x.verified_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "created_at",
                value: new DateTime(2026, 6, 12, 3, 17, 35, 872, DateTimeKind.Utc).AddTicks(1318));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "created_at",
                value: new DateTime(2026, 6, 12, 3, 17, 35, 872, DateTimeKind.Utc).AddTicks(2434));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "created_at",
                value: new DateTime(2026, 6, 12, 3, 17, 35, 872, DateTimeKind.Utc).AddTicks(2438));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "created_at",
                value: new DateTime(2026, 6, 12, 3, 17, 35, 872, DateTimeKind.Utc).AddTicks(2440));

            migrationBuilder.CreateIndex(
                name: "idx_boat_certificates_boat_expiry",
                table: "boat_certificates",
                columns: new[] { "boat_id", "expiry_date" });

            migrationBuilder.CreateIndex(
                name: "IX_boat_certificates_verified_by",
                table: "boat_certificates",
                column: "verified_by");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "boat_certificates");

            migrationBuilder.DropColumn(
                name: "compliance_status",
                table: "boats");

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
    }
}
