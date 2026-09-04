using DDMS.Backend.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260904130000_ThemPhieuSuaDichVu")]
public partial class ThemPhieuSuaDichVu : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "service_change_requests",
            columns: table => new
            {
                id = table.Column<Guid>(type: "char(36)", nullable: false),
                tour_id = table.Column<Guid>(type: "char(36)", nullable: false),
                boat_id = table.Column<Guid>(type: "char(36)", nullable: false),
                owner_id = table.Column<Guid>(type: "char(36)", nullable: false),
                payload_json = table.Column<string>(type: "json", nullable: false),
                status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValueSql: "'pending'"),
                rejection_reason = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTime>(type: "datetime(6)", maxLength: 6, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                updated_at = table.Column<DateTime>(type: "datetime(6)", maxLength: 6, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
            },
            constraints: table =>
            {
                table.PrimaryKey("PRIMARY", x => x.id);
                table.ForeignKey(
                    name: "fk_service_change_tour",
                    column: x => x.tour_id,
                    principalTable: "tours",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_service_change_boat",
                    column: x => x.boat_id,
                    principalTable: "boats",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "idx_service_change_tour",
            table: "service_change_requests",
            column: "tour_id");

        migrationBuilder.CreateIndex(
            name: "idx_service_change_boat",
            table: "service_change_requests",
            column: "boat_id");

        migrationBuilder.CreateIndex(
            name: "idx_service_change_status",
            table: "service_change_requests",
            column: "status");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "service_change_requests");
    }
}
