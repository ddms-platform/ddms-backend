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

    /// <summary>Legacy tour module (KIỆT) — used by Services/Implementations and public tour controllers.</summary>
    public const int TourNotFound = 1500;
    public const int TourInvalidStatus = 1501;
    public const int TourNotExists = 1502;
    public const int ScheduleNotFound = 1503;
    public const int RouteNotFound = 1504;
    public const int BoatNotExists = 1505;
    public const int DockNotExists = 1506;
    public const int TourImageNotFound = 1507;
    public const int FaqNotFound = 1508;
    public const int DockScheduleNotFound = 1509;
    public const int MaintenanceNotFound = 1510;
    public const int CertificateNotFound = 1511;
    public const int CertificateTypeRequired = 1512;
    public const int CertificateExpiryInvalid = 1513;
    public const int CertificateAlreadyExists = 1514;
    public const int CertificateNotApproved = 1515;
    public const int BoatBlockedCompliance = 1516;
    public const int CertificateTypeNotFound = 1517;
    public const int CertificateTypeCodeExists = 1518;
    public const int HoldNotAllowed = 1519;
    public const int HoldExpired = 1520;

    public const int OwnerDocumentTypeRequired = 1520;
    public const int OwnerDocumentRequired = 1521;
    public const int InvalidOwnerEntityType = 1522;
    public const int OwnerDocumentDeadlineExpired = 1523;
    public const int OwnerDocumentOverdueBlocked = 1524;

    public const int PromotionCodeRequired = 1600;
    public const int PromotionCodeExists = 1601;
    public const int PromotionNotFound = 1602;
    public const int PromotionInactive = 1603;
    public const int PromotionNotStarted = 1604;
    public const int PromotionExpired = 1605;
    public const int PromotionUsageExhausted = 1606;
    public const int PromotionMinOrderNotMet = 1607;
    public const int PromotionNotApplicableToTour = 1608;

    public const int WithdrawalNotFound = 1700;
    public const int WithdrawalAlreadyProcessed = 1701;
    public const int WithdrawAmountInvalid = 1702;
    public const int WithdrawBankInfoRequired = 1703;
    public const int WithdrawInsufficientBalance = 1704;
    public const int WithdrawalLockedDueToDocuments = 1705;

    public const int BookingCheckInPending = 1800;
    public const int BookingCheckInCompleted = 1801;
    public const int BookingCheckInNotEligible = 1802;
    public const int BookingCheckInInvalidCode = 1803;
    public const int BookingCheckInNotFound = 1804;
    public const int BookingCheckInAlreadyCheckedIn = 1805;
    public const int BookingCheckInOwnerCancelled = 1806;
    public const int BookingCheckInCancelled = 1807;

    public const int BookingPaymentNotConfigured = 1900;
    public const int BookingPaymentNotPayable = 1901;
    public const int BookingPaymentGatewayError = 1902;
    public const int BookingPaymentNotFound = 1903;
    public const int BookingPaymentNotPaid = 1904;
    public const int BookingPaymentSimulateDisabled = 1905;

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
    public const int ScheduleBoatOverlap = 2206;
    public const int ScheduleDockOverlap = 2207;
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

    public const int ChatValidationFailed = 2700;

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

        public const string TourNotFound = "Tour not found";
        public const string TourInvalidStatus = "Invalid tour status";
        public const string TourNotExists = "Tour does not exist";
        public const string ScheduleNotFound = "Schedule not found";
        public const string RouteNotFound = "Route not found";
        public const string BoatNotExists = "Boat does not exist";
        public const string DockNotExists = "Dock does not exist";
        public const string TourImageNotFound = "Tour image not found";
        public const string FaqNotFound = "FAQ not found";
        public const string DockScheduleNotFound = "Dock schedule not found";
        public const string MaintenanceNotFound = "Không tìm thấy yêu cầu bảo trì.";

        public const string CertificateNotFound = "Không tìm thấy giấy tờ.";
        public const string CertificateTypeRequired = "Loại giấy tờ là bắt buộc.";
        public const string CertificateExpiryInvalid = "Ngày hết hạn không hợp lệ.";
        public const string CertificateAlreadyExists = "Giấy tờ loại này đã tồn tại cho tàu.";
        public const string CertificateNotApproved = "Giấy tờ chưa được duyệt.";
        public const string BoatBlockedCompliance = "Tàu bị khóa do vi phạm tuân thủ pháp lý.";
        public const string CertificateTypeNotFound = "Không tìm thấy loại giấy tờ.";
        public const string CertificateTypeCodeExists = "Mã loại giấy tờ đã tồn tại.";
        public const string HoldNotAllowed = "Tour khởi hành quá gần, không thể giữ chỗ. Vui lòng thanh toán ngay.";
        public const string HoldExpired = "Chỗ giữ đã hết hạn, vui lòng đặt lại.";

        public const string OwnerDocumentTypeRequired = "Loại giấy tờ chủ thuyền là bắt buộc.";
        public const string OwnerDocumentRequired = "Thiếu giấy tờ bắt buộc của chủ thuyền.";
        public const string InvalidOwnerEntityType = "Loại chủ thể không hợp lệ.";
        public const string OwnerDocumentDeadlineExpired = "Thời hạn bổ sung giấy tờ pháp lý của bạn đã kết thúc. Vui lòng liên hệ Quản trị viên để được hỗ trợ gia hạn.";
        public const string OwnerDocumentOverdueBlocked = "Tài khoản của bạn chưa hoàn tất giấy tờ pháp lý bắt buộc (đã quá thời hạn nộp). Chức năng mở bán tour đã tạm khóa. Vui lòng bổ sung giấy tờ hoặc liên hệ Quản trị viên để được gia hạn.";

        public const string PromotionCodeRequired = "Mã giảm giá không được để trống.";
        public const string PromotionCodeExists = "Mã giảm giá này đã tồn tại trên hệ thống.";
        public const string PromotionNotFound = "Không tìm thấy mã giảm giá.";
        public const string PromotionInactive = "Mã giảm giá này hiện không còn hiệu lực.";
        public const string PromotionNotStarted = "Mã giảm giá này chưa đến ngày áp dụng.";
        public const string PromotionExpired = "Mã giảm giá này đã hết hạn.";
        public const string PromotionUsageExhausted = "Mã giảm giá này đã hết lượt sử dụng.";
        public const string PromotionMinOrderNotMet = "Đơn hàng chưa đạt giá trị tối thiểu để dùng mã này.";
        public const string PromotionNotApplicableToTour = "Mã giảm giá này không áp dụng cho tour đã chọn.";

        public const string WithdrawalNotFound = "Không tìm thấy yêu cầu rút tiền.";
        public const string WithdrawalAlreadyProcessed = "Yêu cầu rút tiền này đã được xử lý trước đó.";
        public const string WithdrawAmountInvalid = "Số tiền rút phải lớn hơn 0.";
        public const string WithdrawBankInfoRequired = "Vui lòng nhập đầy đủ thông tin ngân hàng.";
        public const string WithdrawInsufficientBalance = "Số dư ví không đủ để thực hiện giao dịch.";
        public const string WithdrawalLockedDueToDocuments = "Chức năng rút tiền tạm khóa do hồ sơ pháp lý của bạn chưa hoàn tất và đã quá thời hạn. Vui lòng bổ sung đầy đủ giấy tờ hoặc liên hệ Ban quản trị để được hỗ trợ.";

        public const string BookingCheckInPending = "Vé chưa thanh toán hoặc chưa được xác nhận.";
        public const string BookingCheckInCompleted = "Vé đã hoàn thành, không thể check-in.";
        public const string BookingCheckInNotEligible = "Vé không đủ điều kiện check-in.";
        public const string BookingCheckInInvalidCode = "Mã vé không hợp lệ.";
        public const string BookingCheckInNotFound = "Không tìm thấy vé tương ứng với mã QR.";
        public const string BookingCheckInAlreadyCheckedIn = "Vé đã được check-in trước đó.";
        public const string BookingCheckInOwnerCancelled = "Vé đã bị chủ tour hủy, không thể check-in.";
        public const string BookingCheckInCancelled = "Vé đã bị hủy, không thể check-in.";

        public const string BookingPaymentNotConfigured =
            "Cổng thanh toán PayOS chưa được cấu hình trên máy chủ.";
        public const string BookingPaymentNotPayable =
            "Đơn này không ở trạng thái chờ thanh toán.";
        public const string BookingPaymentGatewayError =
            "Không tạo được yêu cầu thanh toán. Vui lòng thử lại sau ít phút.";
        public const string BookingPaymentNotFound =
            "Đơn này chưa có yêu cầu thanh toán nào.";
        public const string BookingPaymentNotPaid =
            "Chưa nhận được thanh toán cho đơn này.";
        public const string BookingPaymentSimulateDisabled =
            "Giả lập thanh toán chỉ dùng được ở môi trường phát triển.";

        public const string DockScheduleOverlap = "Dock schedule overlap";

        public const string CannotModifySelf = "Cannot modify your own account";
        public const string InvalidRole = "Invalid role";

        /// <summary>Tour module: resource keys (TourResources.resx).</summary>
        public const string TourValidationFailed = nameof(TourValidationFailed);
        public const string TourNameRequired = nameof(TourNameRequired);
        public const string TourPriceInvalid = nameof(TourPriceInvalid);
        public const string TourDurationInvalid = nameof(TourDurationInvalid);
        public const string TourStatusInvalid = nameof(TourStatusInvalid);
        public const string TourCancelPolicyInvalid = nameof(TourCancelPolicyInvalid);
        public const string TourCancelHoursInvalid = nameof(TourCancelHoursInvalid);
        public const string ScheduleStatusInvalid = nameof(ScheduleStatusInvalid);
        public const string ScheduleTimeInvalid = nameof(ScheduleTimeInvalid);
        public const string ScheduleTourNotFound = nameof(ScheduleTourNotFound);
        public const string ScheduleBoatNotFound = nameof(ScheduleBoatNotFound);
        public const string ScheduleDockNotFound = nameof(ScheduleDockNotFound);
        public const string ScheduleBoatOverlap = nameof(ScheduleBoatOverlap);
        public const string ScheduleDockOverlap = nameof(ScheduleDockOverlap);
        public const string RouteTourNotFound = nameof(RouteTourNotFound);
        public const string RouteStartPointRequired = nameof(RouteStartPointRequired);
        public const string RouteEndPointRequired = nameof(RouteEndPointRequired);
        public const string RouteSortOrderInvalid = nameof(RouteSortOrderInvalid);
        public const string TourSearchSortInvalid = nameof(TourSearchSortInvalid);
        public const string TourSearchSortOrderInvalid = nameof(TourSearchSortOrderInvalid);
        public const string TourSearchPriceRangeInvalid = nameof(TourSearchPriceRangeInvalid);
        public const string TourSearchDurationRangeInvalid = nameof(TourSearchDurationRangeInvalid);
        public const string TourImageFileRequired = nameof(TourImageFileRequired);
        public const string TourImageTourNotFound = nameof(TourImageTourNotFound);
        public const string TourImageUploadFailed = nameof(TourImageUploadFailed);
        public const string TourImageSortOrderInvalid = nameof(TourImageSortOrderInvalid);
        public const string FaqQuestionRequired = nameof(FaqQuestionRequired);
        public const string FaqAnswerRequired = nameof(FaqAnswerRequired);
        public const string FaqSortOrderInvalid = nameof(FaqSortOrderInvalid);
        public const string FaqTourNotFound = nameof(FaqTourNotFound);
        public const string DockScheduleTimeInvalid = nameof(DockScheduleTimeInvalid);
        public const string DockScheduleBoatOverlap = nameof(DockScheduleBoatOverlap);
        public const string DockScheduleDockCapacityExceeded = nameof(DockScheduleDockCapacityExceeded);
        public const string DockScheduleBoatNotFound = nameof(DockScheduleBoatNotFound);
        public const string DockScheduleDockNotFound = nameof(DockScheduleDockNotFound);


        public const string UncategorizedError = "Uncategorized error";
    }

    public static bool IsTourModuleError(int code) => code is >= 2100 and <= 2605;
}