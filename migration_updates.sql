-- SQL script to apply all table and column updates added during development
-- This script is designed to be idempotent (safe to run multiple times)

START TRANSACTION;

-- Helper stored procedures for idempotent schema modifications
DROP PROCEDURE IF EXISTS AddColumnIfNotExists;
DROP PROCEDURE IF EXISTS AddIndexIfNotExists;
DROP PROCEDURE IF EXISTS AddForeignKeyIfNotExists;

DELIMITER //

CREATE PROCEDURE AddColumnIfNotExists(
    IN tableName VARCHAR(255),
    IN columnName VARCHAR(255),
    IN columnDefinition VARCHAR(1000)
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = DATABASE() 
        AND table_name = tableName 
        AND column_name = columnName
    ) THEN
        SET @sql = CONCAT('ALTER TABLE `', tableName, '` ADD COLUMN `', columnName, '` ', columnDefinition);
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END //

CREATE PROCEDURE AddIndexIfNotExists(
    IN tableName VARCHAR(255),
    IN indexName VARCHAR(255),
    IN columnName VARCHAR(255),
    IN isUnique BOOLEAN
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.statistics 
        WHERE table_schema = DATABASE() 
        AND table_name = tableName 
        AND index_name = indexName
    ) THEN
        IF isUnique THEN
            SET @sql = CONCAT('CREATE UNIQUE INDEX `', indexName, '` ON `', tableName, '` (', columnName, ')');
        ELSE
            SET @sql = CONCAT('CREATE INDEX `', indexName, '` ON `', tableName, '` (', columnName, ')');
        END IF;
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END //

CREATE PROCEDURE AddForeignKeyIfNotExists(
    IN tableName VARCHAR(255),
    IN constraintName VARCHAR(255),
    IN fkDefinition VARCHAR(1000)
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE table_schema = DATABASE() 
        AND table_name = tableName 
        AND constraint_name = constraintName
    ) THEN
        SET @sql = CONCAT('ALTER TABLE `', tableName, '` ADD CONSTRAINT `', constraintName, '` ', fkDefinition);
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END //

DELIMITER ;

-- 1. Update owner_profiles and boats tables for Vessel Owner registration info
CALL AddColumnIfNotExists('owner_profiles', 'status', "varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL DEFAULT 'Pending'");

CALL AddColumnIfNotExists('boats', 'beam', 'decimal(10,2) NULL');
CALL AddColumnIfNotExists('boats', 'document_url', 'varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL');
CALL AddColumnIfNotExists('boats', 'expected_docking_date', 'datetime(6) NULL');
CALL AddColumnIfNotExists('boats', 'length', 'decimal(10,2) NULL');
CALL AddColumnIfNotExists('boats', 'mooring_type', 'varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL');
CALL AddColumnIfNotExists('boats', 'registration_number', 'varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL');
CALL AddColumnIfNotExists('boats', 'required_services', 'longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL');

-- 2. Create boat_types table and seed initial data
CREATE TABLE IF NOT EXISTS `boat_types` (
    `id` int NOT NULL AUTO_INCREMENT,
    `code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `name_vi` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `name_en` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `boat_types` (`id`, `code`, `name_en`, `name_vi`)
VALUES 
(1, 'catamaran', 'Catamaran', 'Thuyền hai thân'),
(2, 'fishing_boat', 'Fishing Boat', 'Thuyền đánh cá'),
(3, 'speedboat', 'Speedboat', 'Cano'),
(4, 'cruiser', 'Medium Cruiser', 'Tàu du lịch cỡ vừa'),
(5, 'yacht', 'Yacht', 'Du thuyền')
ON DUPLICATE KEY UPDATE `code`=VALUES(`code`), `name_en`=VALUES(`name_en`), `name_vi`=VALUES(`name_vi`);

-- 3. Add map_url to tours table for route mapping
CALL AddColumnIfNotExists('tours', 'map_url', 'longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL');

-- 4. Add image_url to boat cabins and boat services tables
CALL AddColumnIfNotExists('boat_cabins', 'image_url', 'longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL');
CALL AddColumnIfNotExists('boat_services', 'image_url', 'longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL');

-- 5. Create port_maintenance_service table and seed data
CREATE TABLE IF NOT EXISTS `port_maintenance_service` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `icon_code` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `price` decimal(65,30) NULL,
    `description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `port_maintenance_service` (`id`, `created_at`, `description`, `icon_code`, `name`, `price`)
VALUES 
('11111111-1111-1111-1111-111111111111', CURRENT_TIMESTAMP(6), NULL, 'Settings', 'Bảo trì định kỳ', 1200000.0),
('22222222-2222-2222-2222-222222222222', CURRENT_TIMESTAMP(6), NULL, 'AlertTriangle', 'Sửa chữa khẩn cấp', NULL),
('33333333-3333-3333-3333-333333333333', CURRENT_TIMESTAMP(6), NULL, 'User', 'Vệ sinh thân tàu', 500000.0),
('44444444-4444-4444-4444-444444444444', CURRENT_TIMESTAMP(6), NULL, 'Zap', 'Kiểm tra hệ thống điện', 300000.0)
ON DUPLICATE KEY UPDATE `name`=VALUES(`name`), `icon_code`=VALUES(`icon_code`), `price`=VALUES(`price`);

-- 6. Add port_maintenance_service_id and status columns to boat_maintenances table
CALL AddColumnIfNotExists('boat_maintenances', 'port_maintenance_service_id', 'char(36) COLLATE ascii_general_ci NULL');
CALL AddColumnIfNotExists('boat_maintenances', 'status', "varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'pending'");

CALL AddIndexIfNotExists('boat_maintenances', 'IX_boat_maintenances_port_maintenance_service_id', '`port_maintenance_service_id`', FALSE);

CALL AddForeignKeyIfNotExists('boat_maintenances', 'fk_maintenance_port_service', 'FOREIGN KEY (`port_maintenance_service_id`) REFERENCES `port_maintenance_service` (`id`) ON DELETE SET NULL');

-- 7. Add is_deleted columns to support soft deletion for boats and maintenances
CALL AddColumnIfNotExists('boats', 'is_deleted', 'tinyint(1) NOT NULL DEFAULT FALSE');
CALL AddColumnIfNotExists('boat_maintenances', 'is_deleted', 'tinyint(1) NOT NULL DEFAULT FALSE');

-- 8. Create owner_payment table for boat owners' PayOS payments
CREATE TABLE IF NOT EXISTS `owner_payment` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `owner_id` char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `amount` decimal(18,2) NOT NULL,
    `status` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `payos_order_code` bigint NOT NULL,
    `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) NOT NULL,
    `paid_at` datetime(6) NULL,
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_owner_payments_owner` FOREIGN KEY (`owner_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CALL AddIndexIfNotExists('owner_payment', 'IX_owner_payment_owner_id', '`owner_id`', FALSE);

-- 9. Create audit_logs table for system auditing
CREATE TABLE IF NOT EXISTS `audit_logs` (
    `id` char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `user_id` char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `table_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `record_id` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `action` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `old_values` json NULL,
    `new_values` json NULL,
    `ip_address` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_audit_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CALL AddIndexIfNotExists('audit_logs', 'fk_audit_user', '`user_id`', FALSE);
CALL AddIndexIfNotExists('audit_logs', 'idx_audit_table_record', '`table_name`, `record_id`', FALSE);

-- 10. Create user_wallets and wallet_withdrawals tables for cancellation policies and refunds
CREATE TABLE IF NOT EXISTS `user_wallets` (
    `id` char(36) COLLATE utf8mb4_unicode_ci NOT NULL,
    `user_id` char(36) COLLATE utf8mb4_unicode_ci NOT NULL,
    `balance` decimal(12,2) NOT NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CONSTRAINT `PK_user_wallets` PRIMARY KEY (`id`),
    CONSTRAINT `FK_user_wallets_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CALL AddIndexIfNotExists('user_wallets', 'IX_user_wallets_user_id', '`user_id`', TRUE);

CREATE TABLE IF NOT EXISTS `wallet_withdrawals` (
    `id` char(36) COLLATE utf8mb4_unicode_ci NOT NULL,
    `user_id` char(36) COLLATE utf8mb4_unicode_ci NOT NULL,
    `amount` decimal(12,2) NOT NULL,
    `bank_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `account_number` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `account_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'pending',
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `processed_at` datetime(6) NULL,
    CONSTRAINT `PK_wallet_withdrawals` PRIMARY KEY (`id`),
    CONSTRAINT `FK_wallet_withdrawals_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CALL AddIndexIfNotExists('wallet_withdrawals', 'IX_wallet_withdrawals_user_id', '`user_id`', FALSE);

-- 11. Add status column to promotions table
CALL AddColumnIfNotExists('promotions', 'status', "varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'approved'");

-- Clean up helper stored procedures
DROP PROCEDURE IF EXISTS AddColumnIfNotExists;
DROP PROCEDURE IF EXISTS AddIndexIfNotExists;
DROP PROCEDURE IF EXISTS AddForeignKeyIfNotExists;

COMMIT;
