using DDMS.Backend.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260825100000_AllowPendingBoatStatusConstraint")]
    public class AllowPendingBoatStatusConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Schema gốc Workbench: boats_chk_1 = max_passengers > 0,
            // chk_boat_status = status IN ('idle','running').
            // Become-owner ghi status = 'Pending'; admin reject ghi 'rejected'.
            // Drop mọi CHECK trên boats rồi gắn lại tên cố định.
            migrationBuilder.Sql(@"
SET @drop_sql := (
  SELECT CONCAT('ALTER TABLE `boats` ',
    GROUP_CONCAT(CONCAT('DROP CHECK `', CONSTRAINT_NAME, '`') SEPARATOR ', '))
  FROM information_schema.TABLE_CONSTRAINTS
  WHERE CONSTRAINT_SCHEMA = DATABASE()
    AND TABLE_NAME = 'boats'
    AND CONSTRAINT_TYPE = 'CHECK'
);
SET @drop_sql := IFNULL(@drop_sql, 'SELECT 1');
PREPARE stmt FROM @drop_sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
            migrationBuilder.Sql(@"
ALTER TABLE `boats`
  ADD CONSTRAINT `chk_boat_status`
    CHECK (`status` IN ('idle', 'running', 'Pending', 'rejected')),
  ADD CONSTRAINT `chk_boats_max_passengers`
    CHECK (`max_passengers` > 0);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE `boats`
  DROP CHECK `chk_boat_status`,
  DROP CHECK `chk_boats_max_passengers`;
");
            migrationBuilder.Sql(@"
ALTER TABLE `boats`
  ADD CONSTRAINT `chk_boat_status` CHECK (`status` IN ('idle', 'running')),
  ADD CONSTRAINT `chk_boats_max_passengers` CHECK (`max_passengers` > 0);
");
        }
    }
}
