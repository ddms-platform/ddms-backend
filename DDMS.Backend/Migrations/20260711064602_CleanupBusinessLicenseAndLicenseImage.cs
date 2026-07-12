using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class CleanupBusinessLicenseAndLicenseImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: keep boat business_license soft-disabled after owner transport_license migration.
            migrationBuilder.Sql("""
                UPDATE `certificate_types`
                SET `is_active` = 0
                WHERE `code` = 'business_license'
                  AND `scope` = 'boat';
                """);

            // Compat: populate legacy license_image from national_id owner documents when empty.
            migrationBuilder.Sql("""
                UPDATE `owner_profiles` op
                INNER JOIN (
                    SELECT
                        `owner_profile_id`,
                        `document_url`,
                        ROW_NUMBER() OVER (
                            PARTITION BY `owner_profile_id`
                            ORDER BY `updated_at` DESC, `created_at` DESC
                        ) AS rn
                    FROM `owner_documents`
                    WHERE `document_type` = 'national_id'
                ) src ON src.`owner_profile_id` = op.`id` AND src.rn = 1
                SET op.`license_image` = src.`document_url`
                WHERE op.`license_image` IS NULL
                   OR op.`license_image` = '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data cleanup is not reversed; re-enable only if rolling back the whole owner-docs feature.
            migrationBuilder.Sql("""
                UPDATE `certificate_types`
                SET `is_active` = 1
                WHERE `code` = 'business_license'
                  AND `scope` = 'boat';
                """);
        }
    }
}
