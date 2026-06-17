using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBoatTypesSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "boat_types",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "boat_types",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.UpdateData(
                table: "boat_types",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "code", "name_en", "name_vi" },
                values: new object[] { "catamaran", "Catamaran", "Thuyền hai thân" });

            migrationBuilder.UpdateData(
                table: "boat_types",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "code", "name_en", "name_vi" },
                values: new object[] { "fishing_boat", "Fishing Boat", "Thuyền đánh cá" });

            migrationBuilder.UpdateData(
                table: "boat_types",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "code", "name_en", "name_vi" },
                values: new object[] { "speedboat", "Speedboat", "Cano" });

            migrationBuilder.UpdateData(
                table: "boat_types",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "code", "name_en", "name_vi" },
                values: new object[] { "cruiser", "Medium Cruiser", "Tàu du lịch cỡ vừa" });

            migrationBuilder.UpdateData(
                table: "boat_types",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "code", "name_en", "name_vi" },
                values: new object[] { "yacht", "Yacht", "Du thuyền" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "boat_types",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "code", "name_en", "name_vi" },
                values: new object[] { "yacht", "Yacht", "Du thuyền cá nhân (Yacht)" });

            migrationBuilder.UpdateData(
                table: "boat_types",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "code", "name_en", "name_vi" },
                values: new object[] { "catamaran", "Catamaran", "Thuyền hai thân (Catamaran)" });

            migrationBuilder.UpdateData(
                table: "boat_types",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "code", "name_en", "name_vi" },
                values: new object[] { "sailboat", "Sailboat", "Thuyền buồm (Sailboat)" });

            migrationBuilder.UpdateData(
                table: "boat_types",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "code", "name_en", "name_vi" },
                values: new object[] { "speedboat", "Speedboat", "Cano cao tốc (Speedboat)" });

            migrationBuilder.UpdateData(
                table: "boat_types",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "code", "name_en", "name_vi" },
                values: new object[] { "dinghy", "Dinghy/Tender", "Thuyền nhỏ/Thuyền phao (Dinghy/Tender)" });

            migrationBuilder.InsertData(
                table: "boat_types",
                columns: new[] { "id", "code", "name_en", "name_vi" },
                values: new object[,]
                {
                    { 6, "cruiser", "Cruiser", "Tàu du lịch cỡ vừa (Cruiser)" },
                    { 7, "pontoon", "Pontoon", "Thuyền phao sàn bằng (Pontoon)" }
                });
        }
    }
}
