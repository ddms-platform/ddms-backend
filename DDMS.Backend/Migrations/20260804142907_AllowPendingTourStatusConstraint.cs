using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AllowPendingTourStatusConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `tours` DROP CHECK `chk_tours_status`;");
            migrationBuilder.Sql("ALTER TABLE `tours` ADD CONSTRAINT `chk_tours_status` CHECK (`status` IN ('active', 'inactive', 'pending', 'rejected', 'draft', 'deleted'));");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "wallet_withdrawals",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldCollation: "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "wallet_withdrawals",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldCollation: "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "user_wallets",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldCollation: "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "user_wallets",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldCollation: "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "owner_id",
                table: "owner_payment",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldCollation: "utf8mb4_unicode_ci")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `tours` DROP CHECK `chk_tours_status`;");
            migrationBuilder.Sql("ALTER TABLE `tours` ADD CONSTRAINT `chk_tours_status` CHECK (`status` IN ('active', 'inactive'));");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "wallet_withdrawals",
                type: "char(36)",
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "wallet_withdrawals",
                type: "char(36)",
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "user_wallets",
                type: "char(36)",
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "user_wallets",
                type: "char(36)",
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "owner_id",
                table: "owner_payment",
                type: "char(36)",
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "created_at",
                value: new DateTime(2026, 7, 2, 12, 43, 33, 104, DateTimeKind.Utc).AddTicks(5221));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "created_at",
                value: new DateTime(2026, 7, 2, 12, 43, 33, 104, DateTimeKind.Utc).AddTicks(6388));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "created_at",
                value: new DateTime(2026, 7, 2, 12, 43, 33, 104, DateTimeKind.Utc).AddTicks(6394));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "created_at",
                value: new DateTime(2026, 7, 2, 12, 43, 33, 104, DateTimeKind.Utc).AddTicks(6396));
        }
    }
}
