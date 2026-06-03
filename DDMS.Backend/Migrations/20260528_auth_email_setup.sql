-- Auth: email verification + password reset tokens
-- purposes: email_verify | password_reset
ALTER TABLE users
    ADD COLUMN email_verified_at DATETIME(6) NULL AFTER google_id;
DROP TABLE IF EXISTS email_otps;
CREATE TABLE IF NOT EXISTS email_verification_tokens (
    id CHAR(36) NOT NULL,
    email VARCHAR(255) NOT NULL,
    token_hash VARCHAR(64) NOT NULL,
    purpose VARCHAR(20) NOT NULL,
    expires_at DATETIME(6) NOT NULL,
    used_at DATETIME(6) NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    INDEX idx_email_verify_token_email_purpose (email, purpose),
    INDEX idx_email_verify_token_expires (expires_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
UPDATE users
SET email_verified_at = created_at
WHERE email_verified_at IS NULL;
