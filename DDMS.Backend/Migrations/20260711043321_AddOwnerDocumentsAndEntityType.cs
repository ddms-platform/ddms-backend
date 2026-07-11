using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DDMS.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerDocumentsAndEntityType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: DB may already have entity_type/scope from a partial apply.
            migrationBuilder.Sql("""
                SET @col_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'owner_profiles'
                      AND COLUMN_NAME = 'entity_type'
                );
                SET @sql := IF(@col_exists = 0,
                    'ALTER TABLE `owner_profiles` ADD `entity_type` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT ''individual''',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            migrationBuilder.Sql("""
                SET @col_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'certificate_types'
                      AND COLUMN_NAME = 'scope'
                );
                SET @sql := IF(@col_exists = 0,
                    'ALTER TABLE `certificate_types` ADD `scope` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT ''boat''',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            // Match owner_profiles.id charset/collation so the FK is accepted on existing DBs.
            migrationBuilder.Sql("""
                SET @tbl_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'owner_documents'
                );
                SET @id_charset := (
                    SELECT CHARACTER_SET_NAME FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'owner_profiles'
                      AND COLUMN_NAME = 'id'
                    LIMIT 1
                );
                SET @id_collation := (
                    SELECT COLLATION_NAME FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'owner_profiles'
                      AND COLUMN_NAME = 'id'
                    LIMIT 1
                );
                SET @sql := IF(@tbl_exists > 0, 'SELECT 1', CONCAT(
                    'CREATE TABLE `owner_documents` (',
                    '`id` char(36) CHARACTER SET ', @id_charset, ' COLLATE ', @id_collation, ' NOT NULL,',
                    '`owner_profile_id` char(36) CHARACTER SET ', @id_charset, ' COLLATE ', @id_collation, ' NOT NULL,',
                    '`document_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,',
                    '`document_url` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,',
                    '`public_id` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,',
                    '`expiry_date` date NULL,',
                    '`admin_note` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,',
                    '`created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),',
                    '`updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),',
                    'PRIMARY KEY (`id`),',
                    'CONSTRAINT `fk_owner_documents_owner_profile` ',
                    'FOREIGN KEY (`owner_profile_id`) REFERENCES `owner_profiles` (`id`) ON DELETE CASCADE',
                    ') CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci'
                ));
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            migrationBuilder.Sql("""
                UPDATE `certificate_types` SET `scope` = 'boat' WHERE `id` IN (1, 2, 4, 5);
                UPDATE `certificate_types` SET `scope` = 'boat', `is_active` = 0 WHERE `id` = 3;
                """);

            migrationBuilder.Sql("""
                INSERT INTO `certificate_types` (`id`, `code`, `is_active`, `name_en`, `name_vi`, `scope`, `sort_order`)
                VALUES
                    (6, 'crew_certificate', 1, 'Crew certificate / Skipper certificate', 'Danh bạ thuyền viên / Chứng chỉ người lái', 'boat', 6),
                    (7, 'national_id', 1, 'National ID / Passport', 'CCCD / Hộ chiếu', 'owner', 1),
                    (8, 'transport_license', 1, 'Inland waterway transport license', 'Giấy phép KD vận tải thủy nội địa', 'owner', 2),
                    (9, 'business_registration', 1, 'Business registration certificate', 'Giấy chứng nhận đăng ký doanh nghiệp', 'owner', 3),
                    (10, 'residence_proof', 1, 'Residence proof', 'Giấy tờ cư trú', 'owner', 4),
                    (11, 'authorization_letter', 1, 'Authorization letter', 'Giấy ủy quyền', 'owner', 5)
                ON DUPLICATE KEY UPDATE
                    `code` = VALUES(`code`),
                    `is_active` = VALUES(`is_active`),
                    `name_en` = VALUES(`name_en`),
                    `name_vi` = VALUES(`name_vi`),
                    `scope` = VALUES(`scope`),
                    `sort_order` = VALUES(`sort_order`);
                """);

            migrationBuilder.Sql("""
                SET @idx_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'certificate_types'
                      AND INDEX_NAME = 'idx_certificate_types_scope'
                );
                SET @sql := IF(@idx_exists = 0,
                    'CREATE INDEX `idx_certificate_types_scope` ON `certificate_types` (`scope`)',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            migrationBuilder.Sql("""
                SET @idx_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'owner_documents'
                      AND INDEX_NAME = 'idx_owner_documents_profile_type'
                );
                SET @sql := IF(@idx_exists = 0,
                    'CREATE INDEX `idx_owner_documents_profile_type` ON `owner_documents` (`owner_profile_id`, `document_type`)',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

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
            migrationBuilder.Sql("DROP TABLE IF EXISTS `owner_documents`;");

            migrationBuilder.Sql("""
                SET @idx_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'certificate_types'
                      AND INDEX_NAME = 'idx_certificate_types_scope'
                );
                SET @sql := IF(@idx_exists > 0,
                    'DROP INDEX `idx_certificate_types_scope` ON `certificate_types`',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            migrationBuilder.Sql("""
                DELETE FROM `certificate_types` WHERE `id` IN (6, 7, 8, 9, 10, 11);
                UPDATE `certificate_types` SET `is_active` = 1 WHERE `id` = 3;
                """);

            migrationBuilder.Sql("""
                SET @col_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'owner_profiles'
                      AND COLUMN_NAME = 'entity_type'
                );
                SET @sql := IF(@col_exists > 0,
                    'ALTER TABLE `owner_profiles` DROP COLUMN `entity_type`',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            migrationBuilder.Sql("""
                SET @col_exists := (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'certificate_types'
                      AND COLUMN_NAME = 'scope'
                );
                SET @sql := IF(@col_exists > 0,
                    'ALTER TABLE `certificate_types` DROP COLUMN `scope`',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);
        }
    }
}
