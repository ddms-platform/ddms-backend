namespace DDMS.Backend.Common.Constants;

public static class MessageConstants
{
    public const string ValidationFailed = "Validation failed";
    public const string FullNameRequired = "Full name is required";
    public const string EmailRequired = "Email is required";
    public const string PasswordRequired = "Password is required";
    public const string PasswordMinLength = "Password must be at least 8 characters";
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
    public const string InvalidToken = "Invalid token";
    public const string TokenExpired = "Token expired";
    public const string RefreshTokenInvalid = "Invalid refresh token";
    public const string RefreshTokenExpired = "Refresh token expired";
    public const string RefreshTokenRevoked = "Refresh token revoked";
    public const string Unauthorized = "Unauthorized";
    public const string Forbidden = "Forbidden";
    public const string ResourceNotFound = "Resource not found";
    public const string CannotModifySelf = "Cannot modify your own account";
    public const string InvalidRole = "Invalid role";
    public const string UncategorizedError = "Uncategorized error";
}
