using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSosAlertsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `sos_alerts` (
                    `id` CHAR(36) NOT NULL,
                    `user_id` CHAR(36) NOT NULL,
                    `boat_id` CHAR(36) NULL,
                    `latitude` DECIMAL(10,7) NOT NULL,
                    `longitude` DECIMAL(10,7) NOT NULL,
                    `status` VARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
                    `note` VARCHAR(500) NULL,
                    `created_at` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
                    `resolved_at` DATETIME(6) NULL,
                    `resolved_by` CHAR(36) NULL,
                    PRIMARY KEY (`id`),
                    KEY `idx_sos_alerts_user` (`user_id`),
                    KEY `idx_sos_alerts_boat` (`boat_id`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS `sos_alerts`;");
        }
    }
}
