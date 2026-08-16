using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class ThemKhoangNeoChoLichNeoDau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "berth_code",
                table: "dock_schedules",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "idx_dock_schedules_berth",
                table: "dock_schedules",
                columns: new[] { "dock_id", "berth_code" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_dock_schedules_berth",
                table: "dock_schedules");

            migrationBuilder.DropColumn(
                name: "berth_code",
                table: "dock_schedules");
        }
    }
}
