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

    public const int TourValidationFailed = 2100;
    public const int TourStatusInvalid = 2101;
    public const int TourCancelPolicyInvalid = 2102;
    public const int TourCancelHoursInvalid = 2103;
    public const int ScheduleValidationFailed = 2200;
    public const int ScheduleStatusInvalid = 2201;
    public const int ScheduleTimeInvalid = 2202;
    public const int ScheduleTourNotFound = 2203;
    public const int ScheduleBoatNotFound = 2204;
    public const int ScheduleDockNotFound = 2205;
    public const int RouteValidationFailed = 2300;
    public const int RouteTourNotFound = 2301;
    public const int TourSearchValidationFailed = 2400;
    public const int TourSearchSortInvalid = 2401;
    public const int TourSearchPriceRangeInvalid = 2402;
    public const int TourImageValidationFailed = 2500;
    public const int TourImageTourNotFound = 2501;
    public const int TourImageUploadFailed = 2502;
    public const int FaqValidationFailed = 2503;
    public const int FaqTourNotFound = 2504;
    public const int DockScheduleValidationFailed = 2600;
    public const int DockScheduleTimeInvalid = 2601;
    public const int DockScheduleOverlap = 2602;
    public const int DockScheduleDockCapacityExceeded = 2603;
    public const int DockScheduleBoatNotFound = 2604;
    public const int DockScheduleDockNotFound = 2605;

    public const int UncategorizedError = 9999;

    public static class Messages
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

        public const string PasswordResetTokenRequired = "Password reset token is required";
        public const string PasswordResetTokenInvalid = "Invalid or expired password reset link";
        public const string PasswordResetLinkSent = "If an account exists for this email, a password reset link has been sent";
        public const string PasswordResetSuccess = "Password reset successfully";
        public const string PasswordResetSocialAccount = "This account uses Google sign-in and has no password to reset";

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
        public const string TourNameRequired = "Tour name is required";
        public const string TourPriceInvalid = "Tour price must be greater than or equal to 0";
        public const string TourDurationInvalid = "Tour duration minutes must be greater than 0";
        public const string TourStatusInvalid = "Tour status must be active or inactive";
        public const string TourCancelPolicyInvalid = "Cancel policy must be free, partial, or no_refund";
        public const string TourCancelHoursInvalid = "Cancel hours is required and must be >= 0 when policy is partial or no_refund";
        public const string ScheduleStatusInvalid = "Schedule status must be scheduled, ongoing, completed, or cancelled";
        public const string ScheduleTimeInvalid = "Schedule end time must be greater than start time";
        public const string ScheduleTourNotFound = "Tour not found or not owned by current user";
        public const string ScheduleBoatNotFound = "Boat not found";
        public const string ScheduleDockNotFound = "Dock not found";
        public const string RouteTourNotFound = "Tour not found or not owned by current user";
        public const string RouteStartPointRequired = "Route start point is required";
        public const string RouteEndPointRequired = "Route end point is required";
        public const string RouteSortOrderInvalid = "Route sort order must be greater than or equal to 0";
        public const string TourSearchSortInvalid = "Sort by must be price or rating";
        public const string TourSearchSortOrderInvalid = "Sort order must be asc or desc";
        public const string TourSearchPriceRangeInvalid = "Minimum price must be less than or equal to maximum price";
        public const string TourSearchDurationRangeInvalid = "Minimum duration must be less than or equal to maximum duration";
        public const string TourImageFileRequired = "Image file is required";
        public const string TourImageTourNotFound = "Tour not found or not owned by current user";
        public const string TourImageUploadFailed = "Failed to upload image to Cloudinary";
        public const string TourImageSortOrderInvalid = "Image sort order must be greater than or equal to 0";
        public const string FaqQuestionRequired = "FAQ question is required";
        public const string FaqAnswerRequired = "FAQ answer is required";
        public const string FaqSortOrderInvalid = "FAQ sort order must be greater than or equal to 0";
        public const string FaqTourNotFound = "Tour not found or not owned by current user";
        public const string DockScheduleTimeInvalid = "Dock schedule end time must be greater than start time";
        public const string DockScheduleBoatOverlap = "Boat already has an overlapping dock schedule";
        public const string DockScheduleDockCapacityExceeded = "Dock capacity exceeded for the selected time slot";
        public const string DockScheduleBoatNotFound = "Boat not found";
        public const string DockScheduleDockNotFound = "Dock not found";

        public const string UncategorizedError = "Uncategorized error";
    }
}
