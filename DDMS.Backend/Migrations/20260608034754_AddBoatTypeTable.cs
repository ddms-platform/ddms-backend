using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddBoatTypeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "boat_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_vi = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_en = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.InsertData(
                table: "boat_types",
                columns: new[] { "id", "code", "name_en", "name_vi" },
                values: new object[,]
                {
                    { 1, "yacht", "Yacht", "Du thuyền cá nhân (Yacht)" },
                    { 2, "catamaran", "Catamaran", "Thuyền hai thân (Catamaran)" },
                    { 3, "sailboat", "Sailboat", "Thuyền buồm (Sailboat)" },
                    { 4, "speedboat", "Speedboat", "Cano cao tốc (Speedboat)" },
                    { 5, "dinghy", "Dinghy/Tender", "Thuyền nhỏ/Thuyền phao (Dinghy/Tender)" },
                    { 6, "cruiser", "Cruiser", "Tàu du lịch cỡ vừa (Cruiser)" },
                    { 7, "pontoon", "Pontoon", "Thuyền phao sàn bằng (Pontoon)" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "boat_types");
        }
    }
}
