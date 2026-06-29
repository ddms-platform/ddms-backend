using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWishlistToTourFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("TRUNCATE TABLE `wishlists`;");

        // migrationBuilder.DropForeignKey(
        //     name: "fk_wishlists_boat",
        //     table: "wishlists");

        // migrationBuilder.RenameColumn(
        //     name: "boat_id",
        //     table: "wishlists",
        //     newName: "tour_id");

        // migrationBuilder.RenameIndex(
        //     name: "fk_wishlists_boat",
        //     table: "wishlists",
        //     newName: "fk_wishlists_tour");

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "created_at",
                value: new DateTime(2026, 6, 29, 8, 12, 3, 208, DateTimeKind.Utc).AddTicks(9291));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "created_at",
                value: new DateTime(2026, 6, 29, 8, 12, 3, 209, DateTimeKind.Utc).AddTicks(472));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "created_at",
                value: new DateTime(2026, 6, 29, 8, 12, 3, 209, DateTimeKind.Utc).AddTicks(478));

            migrationBuilder.UpdateData(
                table: "port_maintenance_service",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "created_at",
                value: new DateTime(2026, 6, 29, 8, 12, 3, 209, DateTimeKind.Utc).AddTicks(479));

            migrationBuilder.AddForeignKey(
                name: "fk_wishlists_tour",
                table: "wishlists",
                column: "tour_id",
                principalTable: "tours",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_wishlists_tour",
                table: "wishlists");

            migrationBuilder.RenameColumn(
                name: "tour_id",
                table: "wishlists",
                newName: "boat_id");

            migrationBuilder.RenameIndex(
                name: "fk_wishlists_tour",
                table: "wishlists",
                newName: "fk_wishlists_boat");

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

            migrationBuilder.AddForeignKey(
                name: "fk_wishlists_boat",
                table: "wishlists",
                column: "boat_id",
                principalTable: "boats",
                principalColumn: "id");
        }
    }
}
