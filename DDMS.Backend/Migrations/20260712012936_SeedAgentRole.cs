using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class SeedAgentRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thêm role Đại lý (Agent). Idempotent nhờ unique index trên name.
            migrationBuilder.Sql(
                "INSERT INTO roles (name, description, created_at) " +
                "VALUES ('agent', 'Travel Agency (Đại lý du lịch)', CURRENT_TIMESTAMP(6)) " +
                "ON DUPLICATE KEY UPDATE description = VALUES(description);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM roles WHERE name = 'agent';");
        }
    }
}
