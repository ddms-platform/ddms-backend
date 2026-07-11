using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class SyncLegalCertificateTypeCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "name_en", "name_vi" },
                values: new object[] { "Inland waterway vessel registration certificate", "Giấy chứng nhận đăng ký phương tiện thủy nội địa" });

            migrationBuilder.UpdateData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "name_en", "name_vi" },
                values: new object[] { "Civil liability insurance", "Bảo hiểm trách nhiệm dân sự" });

            migrationBuilder.UpdateData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "name_en", "name_vi" },
                values: new object[] { "Technical safety & environmental protection certificate", "Giấy chứng nhận an toàn kỹ thuật & bảo vệ môi trường (Đăng kiểm)" });

            migrationBuilder.UpdateData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 5,
                column: "sort_order",
                value: 8);

            migrationBuilder.UpdateData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "name_en", "name_vi" },
                values: new object[] { "Transport operation license", "Giấy phép hoạt động vận tải" });

            migrationBuilder.InsertData(
                table: "certificate_types",
                columns: new[] { "id", "code", "is_active", "name_en", "name_vi", "scope", "sort_order" },
                values: new object[] { 12, "fire_safety", true, "Fire safety certificate", "Giấy chứng nhận PCCC", "boat", 7 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 12);

            migrationBuilder.UpdateData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "name_en", "name_vi" },
                values: new object[] { "Maritime registration", "Đăng ký hàng hải" });

            migrationBuilder.UpdateData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "name_en", "name_vi" },
                values: new object[] { "Insurance", "Bảo hiểm" });

            migrationBuilder.UpdateData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "name_en", "name_vi" },
                values: new object[] { "Safety certificate", "Chứng nhận an toàn" });

            migrationBuilder.UpdateData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 5,
                column: "sort_order",
                value: 5);

            migrationBuilder.UpdateData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "name_en", "name_vi" },
                values: new object[] { "Inland waterway transport license", "Giấy phép KD vận tải thủy nội địa" });
        }
    }
}
