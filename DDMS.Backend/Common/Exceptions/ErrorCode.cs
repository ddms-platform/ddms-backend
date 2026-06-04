namespace DDMS.Backend.Common.Exceptions;

public static class ErrorCode
{
    public const int Success = 1000;

    public const int AuthValidationFailed = 1100;
    public const int AuthEmailAlreadyExists = 1200;
    public const int AuthAccountInactive = 1201;
    public const int AuthInvalidCredentials = 1202;
    public const int AuthEmailNotVerified = 1204;
    public const int AuthOtpInvalid = 1205;
    public const int AuthOtpExpired = 1206;
    public const int AuthOtpRateLimited = 1207;
    public const int AuthGoogleTokenInvalid = 1208;
    public const int AuthRateLimited = 1209;

    public const int AuthTokenInvalid = 1300;
    public const int AuthTokenExpired = 1301;
    public const int AuthRefreshTokenInvalid = 1302;
    public const int AuthRefreshTokenExpired = 1303;
    public const int AuthRefreshTokenRevoked = 1304;
    public const int AuthRefreshTokenReuseDetected = 1305;

    public const int AuthUnauthorized = 1401;
    public const int Forbidden = 1403;
    public const int ResourceNotFound = 1404;

    public const int UncategorizedError = 9999;

    public static class Messages
    {
        public const string ValidationFailed = "Validation failed";

        public const string FullNameRequired = "Full name is required";
        public const string EmailRequired = "Email is required";
        public const string PasswordRequired = "Password is required";
        public const string PasswordMinLength = "Password must be at least 8 characters";
        public const string PasswordPolicy = "Password must be at least 8 characters and include lowercase, uppercase, number and a special character";
        public const string ConfirmPasswordMismatch = "Confirm password does not match";

        public const string EmailAlreadyExists = "Email already exists";
        public const string AccountInactive = "Account is inactive";
        public const string InvalidCredentials = "Invalid email or password";
        public const string EmailNotVerified = "Email is not verified";

        public const string VerificationTokenRequired = "Verification token is required";
        public const string VerificationTokenInvalid = "Invalid verification token";
        public const string VerificationTokenExpired = "Verification link expired or already used";
        public const string VerificationRateLimited = "Too many verification requests. Please try again later";

        public const string GoogleTokenInvalid = "Invalid Google token";

        public const string VerificationLinkSent = "Verification link sent to your email";
        public const string CheckEmailForVerification = "Please check your email to verify your account";
        public const string EmailVerified = "Email verified successfully";
        public const string EmailAlreadyVerified = "Email is already verified";

        public const string PasswordResetTokenRequired = "Password reset token is required";
        public const string PasswordResetTokenInvalid = "Invalid or expired password reset link";
        public const string PasswordResetLinkSent = "If an account exists for this email, a password reset link has been sent";
        public const string PasswordResetSuccess = "Password reset successfully";
        public const string PasswordResetSocialAccount = "This account uses Google sign-in and has no password to reset";

        public const string CurrentPasswordRequired = "Current password is required";
        public const string CurrentPasswordIncorrect = "Current password is incorrect";
        public const string NewPasswordSameAsOld = "New password must be different from the current password";
        public const string ChangePasswordSuccess = "Password changed successfully";
        public const string ChangePasswordSocialAccount = "This account uses Google sign-in and has no password to change";

        public const string InvalidToken = "Invalid token";
        public const string TokenExpired = "Token expired";

        public const string RefreshTokenInvalid = "Invalid refresh token";
        public const string RefreshTokenExpired = "Refresh token expired";
        public const string RefreshTokenRevoked = "Refresh token revoked";
        public const string RefreshTokenReuseDetected = "Suspicious session activity detected. Please sign in again";

        public const string AuthRateLimited = "Too many requests. Please try again later";

        public const string Unauthorized = "Unauthorized";
        public const string Forbidden = "Forbidden";
        public const string ResourceNotFound = "Resource not found";

        public const string CannotModifySelf = "Cannot modify your own account";
        public const string InvalidRole = "Invalid role";

        public const string UncategorizedError = "Uncategorized error";
    }
}
