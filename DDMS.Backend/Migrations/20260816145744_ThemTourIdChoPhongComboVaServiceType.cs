using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class ThemTourIdChoPhongComboVaServiceType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "service_type",
                table: "tours",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "tour_id",
                table: "boat_services",
                type: "char(36)",
                nullable: true,
                collation: "utf8mb4_unicode_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "tour_id",
                table: "boat_cabins",
                type: "char(36)",
                nullable: true,
                collation: "utf8mb4_unicode_ci");

            migrationBuilder.CreateIndex(
                name: "IX_boat_services_tour_id",
                table: "boat_services",
                column: "tour_id");

            migrationBuilder.CreateIndex(
                name: "IX_boat_cabins_tour_id",
                table: "boat_cabins",
                column: "tour_id");

            migrationBuilder.AddForeignKey(
                name: "fk_cabins_tour",
                table: "boat_cabins",
                column: "tour_id",
                principalTable: "tours",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_services_tour",
                table: "boat_services",
                column: "tour_id",
                principalTable: "tours",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cabins_tour",
                table: "boat_cabins");

            migrationBuilder.DropForeignKey(
                name: "fk_services_tour",
                table: "boat_services");

            migrationBuilder.DropIndex(
                name: "IX_boat_services_tour_id",
                table: "boat_services");

            migrationBuilder.DropIndex(
                name: "IX_boat_cabins_tour_id",
                table: "boat_cabins");

            migrationBuilder.DropColumn(
                name: "service_type",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "tour_id",
                table: "boat_services");

            migrationBuilder.DropColumn(
                name: "tour_id",
                table: "boat_cabins");
        }
    }
}
