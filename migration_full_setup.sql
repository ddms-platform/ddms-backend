-- =============================================================================
-- FULL DATABASE CREATION SCRIPT
-- This script creates all tables, columns, indexes, foreign keys, and seed data
-- from scratch. Use this only if you are setting up a completely fresh database.
-- =============================================================================

CREATE DATABASE IF NOT EXISTS `boat_tour` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `boat_tour`;

START TRANSACTION;

-- Drop tables if they exist to start fresh
DROP TABLE IF EXISTS `messages`;
DROP TABLE IF EXISTS `conversation_members`;
DROP TABLE IF EXISTS `reviews`;
DROP TABLE IF EXISTS `payments`;
DROP TABLE IF EXISTS `loyalty_points`;
DROP TABLE IF EXISTS `conversations`;
DROP TABLE IF EXISTS `booking_services`;
DROP TABLE IF EXISTS `booking_cabins`;
DROP TABLE IF EXISTS `dock_schedules`;
DROP TABLE IF EXISTS `bookings`;
DROP TABLE IF EXISTS `tour_schedules`;
DROP TABLE IF EXISTS `tour_images`;
DROP TABLE IF EXISTS `routes`;
DROP TABLE IF EXISTS `faqs`;
DROP TABLE IF EXISTS `notification_recipients`;
DROP TABLE IF EXISTS `wishlists`;
DROP TABLE IF EXISTS `boat_services`;
DROP TABLE IF EXISTS `boat_maintenances`;
DROP TABLE IF EXISTS `port_maintenance_service`;
DROP TABLE IF EXISTS `boat_images`;
DROP TABLE IF EXISTS `boat_cabins`;
DROP TABLE IF EXISTS `ai_messages`;
DROP TABLE IF EXISTS `user_roles`;
DROP TABLE IF EXISTS `tours`;
DROP TABLE IF EXISTS `refresh_tokens`;
DROP TABLE IF EXISTS `promotions`;
DROP TABLE IF EXISTS `owner_profiles`;
DROP TABLE IF EXISTS `owner_payment`;
DROP TABLE IF EXISTS `notifications`;
DROP TABLE IF EXISTS `boats`;
DROP TABLE IF EXISTS `audit_logs`;
DROP TABLE IF EXISTS `ai_conversations`;
DROP TABLE IF EXISTS `users`;
DROP TABLE IF EXISTS `roles`;
DROP TABLE IF EXISTS `email_verification_tokens`;
DROP TABLE IF EXISTS `docks`;
DROP TABLE IF EXISTS `boat_types`;
DROP TABLE IF EXISTS `user_wallets`;
DROP TABLE IF EXISTS `wallet_withdrawals`;

-- 1. Create Core Tables
CREATE TABLE `docks` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `location` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `max_boats` int NOT NULL DEFAULT '1',
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `email_verification_tokens` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `email` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `token_hash` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `purpose` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `expires_at` datetime(6) NOT NULL,
    `used_at` datetime(6) NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `roles` (
    `id` int NOT NULL AUTO_INCREMENT,
    `name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `users` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `full_name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `email` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `password_hash` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `phone` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `address` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `avatar_url` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `is_active` tinyint(1) NOT NULL DEFAULT '1',
    `google_id` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `email_verified_at` datetime(6) NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    UNIQUE KEY `email` (`email`),
    UNIQUE KEY `google_id` (`google_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 2. Create Relational Tables
CREATE TABLE `owner_profiles` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `user_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `business_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `bio` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `license_number` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `license_image` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `phone_business` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `address` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `is_verified` tinyint(1) NOT NULL,
    `verified_at` datetime(6) NULL,
    `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL DEFAULT 'Pending',
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_owner_profiles_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
    UNIQUE KEY `user_id` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `boats` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `owner_id` char(36) COLLATE ascii_general_ci NULL,
    `name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `max_passengers` int NOT NULL,
    `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'idle',
    `beam` decimal(10,2) NULL,
    `document_url` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `expected_docking_date` datetime(6) NULL,
    `length` decimal(10,2) NULL,
    `mooring_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `registration_number` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `required_services` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `is_deleted` tinyint(1) NOT NULL DEFAULT FALSE,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_boats_owner` FOREIGN KEY (`owner_id`) REFERENCES `users` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `promotions` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `discount_type` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'percent',
    `discount_value` decimal(12,2) NOT NULL,
    `min_order_value` decimal(12,2) NOT NULL,
    `max_discount` decimal(12,2) NULL,
    `usage_limit` int NULL,
    `used_count` int NOT NULL,
    `valid_from` datetime(6) NOT NULL,
    `valid_until` datetime(6) NULL,
    `is_active` tinyint(1) NOT NULL DEFAULT '1',
    `created_by` char(36) COLLATE ascii_general_ci NULL,
    `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'approved',
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_promotions_created_by` FOREIGN KEY (`created_by`) REFERENCES `users` (`id`) ON DELETE SET NULL,
    UNIQUE KEY `code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `tours` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `price` decimal(12,2) NOT NULL,
    `duration_minutes` int NOT NULL,
    `location` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `avg_rating` decimal(3,2) NOT NULL,
    `total_reviews` int NOT NULL,
    `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'active',
    `cancel_policy` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'free',
    `cancel_hours` int NULL,
    `map_url` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_by` char(36) COLLATE ascii_general_ci NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_tours_created_by` FOREIGN KEY (`created_by`) REFERENCES `users` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `tour_schedules` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `tour_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `boat_id` char(36) COLLATE ascii_general_ci NULL,
    `dock_id` char(36) COLLATE ascii_general_ci NULL,
    `start_time` datetime(6) NOT NULL,
    `end_time` datetime(6) NOT NULL,
    `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'scheduled',
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_schedules_boat` FOREIGN KEY (`boat_id`) REFERENCES `boats` (`id`) ON DELETE SET NULL,
    CONSTRAINT `fk_schedules_dock` FOREIGN KEY (`dock_id`) REFERENCES `docks` (`id`) ON DELETE SET NULL,
    CONSTRAINT `fk_schedules_tour` FOREIGN KEY (`tour_id`) REFERENCES `tours` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `bookings` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `user_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `schedule_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `promotion_id` char(36) COLLATE ascii_general_ci NULL,
    `num_people` int NOT NULL,
    `base_price` decimal(12,2) NOT NULL,
    `cabin_price` decimal(12,2) NOT NULL,
    `service_price` decimal(12,2) NOT NULL,
    `discount_amount` decimal(12,2) NOT NULL,
    `total_price` decimal(12,2) NOT NULL,
    `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'pending',
    `notes` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `cancelled_at` datetime(6) NULL,
    `cancel_reason` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_bookings_promotion` FOREIGN KEY (`promotion_id`) REFERENCES `promotions` (`id`) ON DELETE SET NULL,
    CONSTRAINT `fk_bookings_schedule` FOREIGN KEY (`schedule_id`) REFERENCES `tour_schedules` (`id`),
    CONSTRAINT `fk_bookings_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 3. Create Other Child Tables
CREATE TABLE `boat_cabins` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `boat_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `capacity` int NOT NULL DEFAULT '2',
    `price` decimal(12,2) NOT NULL,
    `total_rooms` int NOT NULL DEFAULT '1',
    `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `image_url` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_cabins_boat` FOREIGN KEY (`boat_id`) REFERENCES `boats` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `boat_services` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `boat_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `price` decimal(12,2) NOT NULL,
    `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `is_active` tinyint(1) NOT NULL DEFAULT '1',
    `image_url` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_services_boat` FOREIGN KEY (`boat_id`) REFERENCES `boats` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `port_maintenance_service` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `icon_code` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `price` decimal(65,30) NULL,
    `description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `boat_maintenances` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `boat_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `start_time` datetime(6) NOT NULL,
    `end_time` datetime(6) NOT NULL,
    `reason` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `port_maintenance_service_id` char(36) COLLATE ascii_general_ci NULL,
    `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'pending',
    `is_deleted` tinyint(1) NOT NULL DEFAULT FALSE,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_maintenance_boat` FOREIGN KEY (`boat_id`) REFERENCES `boats` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_maintenance_port_service` FOREIGN KEY (`port_maintenance_service_id`) REFERENCES `port_maintenance_service` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `boat_types` (
    `id` int NOT NULL AUTO_INCREMENT,
    `code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `name_vi` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `name_en` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `owner_payment` (
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

CREATE TABLE `audit_logs` (
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

CREATE TABLE `user_wallets` (
    `id` char(36) COLLATE utf8mb4_unicode_ci NOT NULL,
    `user_id` char(36) COLLATE utf8mb4_unicode_ci NOT NULL,
    `balance` decimal(12,2) NOT NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CONSTRAINT `PK_user_wallets` PRIMARY KEY (`id`),
    CONSTRAINT `FK_user_wallets_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `wallet_withdrawals` (
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

CREATE TABLE `boat_images` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `boat_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `image_url` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `public_id` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `caption` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `sort_order` int NOT NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_boat_images_boat` FOREIGN KEY (`boat_id`) REFERENCES `boats` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `wishlists` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `user_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `boat_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_wishlists_boat` FOREIGN KEY (`boat_id`) REFERENCES `boats` (`id`),
    CONSTRAINT `fk_wishlists_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `notification_recipients` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `notification_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `user_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `is_read` tinyint(1) NOT NULL,
    `read_at` datetime(6) NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_notif_recipients_notification` FOREIGN KEY (`notification_id`) REFERENCES `notifications` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_notif_recipients_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `faqs` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `tour_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `question` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `answer` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `sort_order` int NOT NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_faqs_tour` FOREIGN KEY (`tour_id`) REFERENCES `tours` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `routes` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `tour_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `start_point` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `end_point` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `sort_order` int NOT NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_routes_tour` FOREIGN KEY (`tour_id`) REFERENCES `tours` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `tour_images` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `tour_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `image_url` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `public_id` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `caption` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `sort_order` int NOT NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_tour_images_tour` FOREIGN KEY (`tour_id`) REFERENCES `tours` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `dock_schedules` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `dock_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `boat_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `schedule_id` char(36) COLLATE ascii_general_ci NULL,
    `start_time` datetime(6) NOT NULL,
    `end_time` datetime(6) NOT NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_dock_sched_boat` FOREIGN KEY (`boat_id`) REFERENCES `boats` (`id`),
    CONSTRAINT `fk_dock_sched_dock` FOREIGN KEY (`dock_id`) REFERENCES `docks` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_dock_sched_schedule` FOREIGN KEY (`schedule_id`) REFERENCES `tour_schedules` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `booking_cabins` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `booking_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `cabin_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `quantity` int NOT NULL DEFAULT '1',
    `unit_price` decimal(12,2) NOT NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_booking_cabins_booking` FOREIGN KEY (`booking_id`) REFERENCES `bookings` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_booking_cabins_cabin` FOREIGN KEY (`cabin_id`) REFERENCES `boat_cabins` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `booking_services` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `booking_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `service_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `quantity` int NOT NULL DEFAULT '1',
    `unit_price` decimal(12,2) NOT NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_booking_services_booking` FOREIGN KEY (`booking_id`) REFERENCES `bookings` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_booking_services_service` FOREIGN KEY (`service_id`) REFERENCES `boat_services` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `conversations` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `type` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'direct',
    `name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `booking_id` char(36) COLLATE ascii_general_ci NULL,
    `schedule_id` char(36) COLLATE ascii_general_ci NULL,
    `created_by` char(36) COLLATE ascii_general_ci NOT NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_conversations_booking` FOREIGN KEY (`booking_id`) REFERENCES `bookings` (`id`) ON DELETE SET NULL,
    CONSTRAINT `fk_conversations_created_by` FOREIGN KEY (`created_by`) REFERENCES `users` (`id`),
    CONSTRAINT `fk_conversations_schedule` FOREIGN KEY (`schedule_id`) REFERENCES `tour_schedules` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `loyalty_points` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `user_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `booking_id` char(36) COLLATE ascii_general_ci NULL,
    `points` int NOT NULL,
    `type` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `note` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_loyalty_booking` FOREIGN KEY (`booking_id`) REFERENCES `bookings` (`id`) ON DELETE SET NULL,
    CONSTRAINT `fk_loyalty_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `payments` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `booking_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `amount` decimal(12,2) NOT NULL,
    `method` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'pending',
    `transaction_ref` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `gateway_response` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `paid_at` datetime(6) NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_payments_booking` FOREIGN KEY (`booking_id`) REFERENCES `bookings` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `reviews` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `user_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `booking_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `tour_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `rating` tinyint NOT NULL,
    `comment` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_reviews_booking` FOREIGN KEY (`booking_id`) REFERENCES `bookings` (`id`),
    CONSTRAINT `fk_reviews_tour` FOREIGN KEY (`tour_id`) REFERENCES `tours` (`id`),
    CONSTRAINT `fk_reviews_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `conversation_members` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `conversation_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `user_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `joined_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `last_read_at` datetime(6) NULL,
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_conv_members_conversation` FOREIGN KEY (`conversation_id`) REFERENCES `conversations` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_conv_members_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `messages` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `conversation_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `sender_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `body` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_messages_conversation` FOREIGN KEY (`conversation_id`) REFERENCES `conversations` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_messages_sender` FOREIGN KEY (`sender_id`) REFERENCES `users` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `notifications` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `type` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `sender_id` char(36) COLLATE ascii_general_ci NULL,
    `title` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `body` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `data` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_notifications_sender` FOREIGN KEY (`sender_id`) REFERENCES `users` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `refresh_tokens` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `user_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `token_hash` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `expires_at` datetime(6) NOT NULL,
    `revoked` tinyint(1) NOT NULL,
    `revoked_at` datetime(6) NULL,
    `user_agent` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `ip_address` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_refresh_tokens_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `user_roles` (
    `user_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `role_id` int NOT NULL,
    `assigned_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`user_id`, `role_id`),
    CONSTRAINT `fk_user_roles_role` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`),
    CONSTRAINT `fk_user_roles_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `ai_conversations` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `user_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `title` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_ai_conversations_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `ai_messages` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `ai_conversation_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `role` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `content` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
    CONSTRAINT `fk_ai_messages_conversation` FOREIGN KEY (`ai_conversation_id`) REFERENCES `ai_conversations` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 4. Create Indexes
CREATE INDEX `idx_ai_conv_user` ON `ai_conversations` (`user_id`);
CREATE INDEX `idx_ai_msg_conv` ON `ai_messages` (`ai_conversation_id`, `created_at`);
CREATE INDEX `fk_audit_user` ON `audit_logs` (`user_id`);
CREATE INDEX `idx_audit_table_record` ON `audit_logs` (`table_name`, `record_id`);
CREATE INDEX `idx_cabins_boat` ON `boat_cabins` (`boat_id`);
CREATE INDEX `idx_boat_images_boat` ON `boat_images` (`boat_id`);
CREATE INDEX `idx_maintenance_boat` ON `boat_maintenances` (`boat_id`, `start_time`, `end_time`);
CREATE INDEX `idx_services_active` ON `boat_services` (`boat_id`, `is_active`);
CREATE INDEX `idx_services_boat` ON `boat_services` (`boat_id`);
CREATE INDEX `idx_boats_owner` ON `boats` (`owner_id`);
CREATE INDEX `fk_booking_cabins_cabin` ON `booking_cabins` (`cabin_id`);
CREATE INDEX `idx_bk_cabins` ON `booking_cabins` (`booking_id`);
CREATE INDEX `fk_booking_services_service` ON `booking_services` (`service_id`);
CREATE INDEX `idx_bk_services` ON `booking_services` (`booking_id`);
CREATE INDEX `fk_bookings_promotion` ON `bookings` (`promotion_id`);
CREATE INDEX `idx_bookings_created` ON `bookings` (`created_at` DESC);
CREATE INDEX `idx_bookings_sched` ON `bookings` (`schedule_id`);
CREATE INDEX `idx_bookings_status` ON `bookings` (`status`);
CREATE INDEX `idx_bookings_user` ON `bookings` (`user_id`);
CREATE INDEX `idx_conv_members` ON `conversation_members` (`user_id`);
CREATE UNIQUE INDEX `uq_conv_member` ON `conversation_members` (`conversation_id`, `user_id`);
CREATE INDEX `idx_conv_booking` ON `conversations` (`booking_id`);
CREATE INDEX `idx_conv_created_by` ON `conversations` (`created_by`);
CREATE INDEX `idx_conv_schedule` ON `conversations` (`schedule_id`);
CREATE INDEX `fk_dock_sched_schedule` ON `dock_schedules` (`schedule_id`);
CREATE INDEX `idx_dock_sched_boat` ON `dock_schedules` (`boat_id`, `start_time`, `end_time`);
CREATE INDEX `idx_dock_sched_dock` ON `dock_schedules` (`dock_id`, `start_time`, `end_time`);
CREATE INDEX `idx_email_verify_token_email_purpose` ON `email_verification_tokens` (`email`, `purpose`);
CREATE INDEX `idx_email_verify_token_expires` ON `email_verification_tokens` (`expires_at`);
CREATE INDEX `idx_faqs_tour` ON `faqs` (`tour_id`);
CREATE INDEX `idx_loyalty_booking` ON `loyalty_points` (`booking_id`);
CREATE INDEX `idx_loyalty_user` ON `loyalty_points` (`user_id`);
CREATE INDEX `idx_messages_conv` ON `messages` (`conversation_id`, `created_at` DESC);
CREATE INDEX `idx_messages_sender` ON `messages` (`sender_id`);
CREATE INDEX `idx_notif_recipient` ON `notification_recipients` (`user_id`, `is_read`);
CREATE UNIQUE INDEX `uq_notif_user` ON `notification_recipients` (`notification_id`, `user_id`);
CREATE INDEX `fk_notifications_sender` ON `notifications` (`sender_id`);
CREATE INDEX `idx_notif_created` ON `notifications` (`created_at` DESC);
CREATE INDEX `idx_owner_verified` ON `owner_profiles` (`is_verified`);
CREATE UNIQUE INDEX `user_id` ON `owner_profiles` (`user_id`);
CREATE UNIQUE INDEX `booking_id` ON `payments` (`booking_id`);
CREATE INDEX `idx_payments_status` ON `payments` (`status`);
CREATE UNIQUE INDEX `code` ON `promotions` (`code`);
CREATE INDEX `fk_promotions_created_by` ON `promotions` (`created_by`);
CREATE INDEX `idx_promotions_active` ON `promotions` (`is_active`, `valid_from`, `valid_until`);
CREATE INDEX `idx_refresh_active` ON `refresh_tokens` (`user_id`, `revoked`);
CREATE INDEX `idx_refresh_user` ON `refresh_tokens` (`user_id`);
CREATE UNIQUE INDEX `token_hash` ON `refresh_tokens` (`token_hash`);
CREATE UNIQUE INDEX `booking_id1` ON `reviews` (`booking_id`);
CREATE INDEX `idx_reviews_tour` ON `reviews` (`tour_id`);
CREATE INDEX `idx_reviews_user` ON `reviews` (`user_id`);
CREATE UNIQUE INDEX `name` ON `roles` (`name`);
CREATE INDEX `fk_routes_tour` ON `routes` (`tour_id`);
CREATE INDEX `fk_tour_images_tour` ON `tour_images` (`tour_id`);
CREATE INDEX `idx_schedules_boat` ON `tour_schedules` (`boat_id`);
CREATE INDEX `idx_schedules_dock` ON `tour_schedules` (`dock_id`);
CREATE INDEX `idx_schedules_status` ON `tour_schedules` (`status`);
CREATE INDEX `idx_schedules_time` ON `tour_schedules` (`start_time`, `end_time`);
CREATE INDEX `idx_schedules_tour` ON `tour_schedules` (`tour_id`);
CREATE INDEX `fk_tours_created_by` ON `tours` (`created_by`);
CREATE INDEX `idx_tours_location` ON `tours` (`location`);
CREATE INDEX `idx_tours_price` ON `tours` (`price`);
CREATE INDEX `idx_tours_rating` ON `tours` (`avg_rating` DESC);
CREATE INDEX `idx_tours_status` ON `tours` (`status`);
CREATE INDEX `fk_user_roles_role` ON `user_roles` (`role_id`);
CREATE UNIQUE INDEX `email` ON `users` (`email`);
CREATE UNIQUE INDEX `google_id` ON `users` (`google_id`);
CREATE INDEX `fk_wishlists_boat` ON `wishlists` (`boat_id`);
CREATE INDEX `idx_wishlists_user` ON `wishlists` (`user_id`);
CREATE UNIQUE INDEX `uq_wishlist` ON `wishlists` (`user_id`, `boat_id`);
CREATE INDEX `IX_owner_payment_owner_id` ON `owner_payment` (`owner_id`);
CREATE UNIQUE INDEX `IX_user_wallets_user_id` ON `user_wallets` (`user_id`);
CREATE INDEX `IX_wallet_withdrawals_user_id` ON `wallet_withdrawals` (`user_id`);

-- 5. Seed Static / Standard Data
INSERT INTO `boat_types` (`id`, `code`, `name_en`, `name_vi`)
VALUES 
(1, 'catamaran', 'Catamaran', 'Thuyền hai thân'),
(2, 'fishing_boat', 'Fishing Boat', 'Thuyền đánh cá'),
(3, 'speedboat', 'Speedboat', 'Cano'),
(4, 'cruiser', 'Medium Cruiser', 'Tàu du lịch cỡ vừa'),
(5, 'yacht', 'Yacht', 'Du thuyền');

INSERT INTO `port_maintenance_service` (`id`, `created_at`, `description`, `icon_code`, `name`, `price`)
VALUES 
('11111111-1111-1111-1111-111111111111', CURRENT_TIMESTAMP(6), NULL, 'Settings', 'Bảo trì định kỳ', 1200000.0),
('22222222-2222-2222-2222-222222222222', CURRENT_TIMESTAMP(6), NULL, 'AlertTriangle', 'Sửa chữa khẩn cấp', NULL),
('33333333-3333-3333-3333-333333333333', CURRENT_TIMESTAMP(6), NULL, 'User', 'Vệ sinh thân tàu', 500000.0),
('44444444-4444-4444-4444-444444444444', CURRENT_TIMESTAMP(6), NULL, 'Zap', 'Kiểm tra hệ thống điện', 300000.0);

COMMIT;
