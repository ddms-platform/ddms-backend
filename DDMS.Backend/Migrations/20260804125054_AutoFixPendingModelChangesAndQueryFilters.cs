using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AutoFixPendingModelChangesAndQueryFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "wallet_withdrawals",
                type: "char(36)",
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldCollation: "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "wallet_withdrawals",
                type: "char(36)",
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldCollation: "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "user_wallets",
                type: "char(36)",
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldCollation: "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "user_wallets",
                type: "char(36)",
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldCollation: "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "owner_id",
                table: "owner_payment",
                type: "char(36)",
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldCollation: "utf8mb4_unicode_ci")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cabins_boat",
                table: "boat_cabins");

            migrationBuilder.DropForeignKey(
                name: "fk_boat_certificates_boat",
                table: "boat_certificates");

            migrationBuilder.DropForeignKey(
                name: "fk_boat_images_boat",
                table: "boat_images");

            migrationBuilder.DropForeignKey(
                name: "fk_services_boat",
                table: "boat_services");

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

            migrationBuilder.AddForeignKey(
                name: "fk_cabins_boat",
                table: "boat_cabins",
                column: "boat_id",
                principalTable: "boats",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_boat_certificates_boat",
                table: "boat_certificates",
                column: "boat_id",
                principalTable: "boats",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_boat_images_boat",
                table: "boat_images",
                column: "boat_id",
                principalTable: "boats",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_services_boat",
                table: "boat_services",
                column: "boat_id",
                principalTable: "boats",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
