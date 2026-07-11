using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerDocumentsAndEntityType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "entity_type",
                table: "owner_profiles",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "individual",
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "scope",
                table: "certificate_types",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "boat",
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "owner_documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    owner_profile_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    document_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    document_url = table.Column<string>(type: "text", nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    public_id = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    expiry_date = table.Column<DateOnly>(type: "date", nullable: true),
                    admin_note = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", maxLength: 6, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", maxLength: 6, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_owner_documents_owner_profile",
                        column: x => x.owner_profile_id,
                        principalTable: "owner_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.UpdateData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 1,
                column: "scope",
                value: "boat");

            migrationBuilder.UpdateData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 2,
                column: "scope",
                value: "boat");

            migrationBuilder.UpdateData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "scope", "is_active" },
                values: new object[] { "boat", false });

            migrationBuilder.UpdateData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 4,
                column: "scope",
                value: "boat");

            migrationBuilder.UpdateData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 5,
                column: "scope",
                value: "boat");

            migrationBuilder.InsertData(
                table: "certificate_types",
                columns: new[] { "id", "code", "is_active", "name_en", "name_vi", "scope", "sort_order" },
                values: new object[,]
                {
                    { 6, "crew_certificate", true, "Crew certificate / Skipper certificate", "Danh bạ thuyền viên / Chứng chỉ người lái", "boat", 6 },
                    { 7, "national_id", true, "National ID / Passport", "CCCD / Hộ chiếu", "owner", 1 },
                    { 8, "transport_license", true, "Inland waterway transport license", "Giấy phép KD vận tải thủy nội địa", "owner", 2 },
                    { 9, "business_registration", true, "Business registration certificate", "Giấy chứng nhận đăng ký doanh nghiệp", "owner", 3 },
                    { 10, "residence_proof", true, "Residence proof", "Giấy tờ cư trú", "owner", 4 },
                    { 11, "authorization_letter", true, "Authorization letter", "Giấy ủy quyền", "owner", 5 }
                });

            migrationBuilder.CreateIndex(
                name: "idx_certificate_types_scope",
                table: "certificate_types",
                column: "scope");

            migrationBuilder.CreateIndex(
                name: "idx_owner_documents_profile_type",
                table: "owner_documents",
                columns: new[] { "owner_profile_id", "document_type" });

            // Best-effort: copy boat business_license certs → owner transport_license docs (one per owner).
            migrationBuilder.Sql("""
                INSERT INTO `owner_documents` (
                    `id`,
                    `owner_profile_id`,
                    `document_type`,
                    `document_url`,
                    `public_id`,
                    `expiry_date`,
                    `admin_note`,
                    `created_at`,
                    `updated_at`
                )
                SELECT
                    UUID(),
                    src.owner_profile_id,
                    'transport_license',
                    src.document_url,
                    src.public_id,
                    src.expiry_date,
                    NULL,
                    src.created_at,
                    src.updated_at
                FROM (
                    SELECT
                        op.id AS owner_profile_id,
                        bc.document_url,
                        bc.public_id,
                        bc.expiry_date,
                        bc.created_at,
                        bc.updated_at,
                        ROW_NUMBER() OVER (
                            PARTITION BY op.id
                            ORDER BY bc.updated_at DESC, bc.created_at DESC
                        ) AS rn
                    FROM `boat_certificates` bc
                    INNER JOIN `boats` b ON b.id = bc.boat_id AND b.owner_id IS NOT NULL
                    INNER JOIN `owner_profiles` op ON op.user_id = b.owner_id
                    WHERE bc.certificate_type = 'business_license'
                ) src
                WHERE src.rn = 1
                  AND NOT EXISTS (
                      SELECT 1
                      FROM `owner_documents` od
                      WHERE od.owner_profile_id = src.owner_profile_id
                        AND od.document_type = 'transport_license'
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "owner_documents");

            migrationBuilder.DropIndex(
                name: "idx_certificate_types_scope",
                table: "certificate_types");

            migrationBuilder.DeleteData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.UpdateData(
                table: "certificate_types",
                keyColumn: "id",
                keyValue: 3,
                column: "is_active",
                value: true);

            migrationBuilder.DropColumn(
                name: "entity_type",
                table: "owner_profiles");

            migrationBuilder.DropColumn(
                name: "scope",
                table: "certificate_types");
        }
    }
}
