namespace DDMS.Backend.Common.Constants;

public static class ErrorCodes
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
    public const int AuthTokenInvalid = 1300;
    public const int AuthTokenExpired = 1301;
    public const int AuthRefreshTokenInvalid = 1302;
    public const int AuthRefreshTokenExpired = 1303;
    public const int AuthRefreshTokenRevoked = 1304;
    public const int AuthUnauthorized = 1401;
    public const int Forbidden = 1403;
    public const int ResourceNotFound = 1404;

    public const int UncategorizedError = 9999;
}
