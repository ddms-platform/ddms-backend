using DDMS.Backend.Common.Localization;
using DDMS.Backend.Common.Responses;
using Microsoft.Extensions.DependencyInjection;

namespace DDMS.Backend.Common.Exceptions;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            throw exception;
        }

        context.Response.ContentType = "application/json";

        var statusCode = StatusCodes.Status500InternalServerError;
        var error = new ApiErrorResponse
        {
            code = ErrorCode.UncategorizedError,
            message = ErrorCode.Messages.UncategorizedError
        };

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = StatusCodes.Status400BadRequest;
                error = new ApiErrorResponse
                {
                    code = validationException.Code,
                    message = validationException.Message,
                    fieldErrors = validationException.FieldErrors
                };
                break;
            case UnauthorizedException unauthorizedException:
                statusCode = StatusCodes.Status401Unauthorized;
                error = new ApiErrorResponse
                {
                    code = unauthorizedException.Code,
                    message = unauthorizedException.Message
                };
                break;
            case ForbiddenException forbiddenException:
                statusCode = StatusCodes.Status403Forbidden;
                error = new ApiErrorResponse
                {
                    code = forbiddenException.Code,
                    message = forbiddenException.Message
                };
                break;
            case NotFoundException notFoundException:
                statusCode = StatusCodes.Status404NotFound;
                error = new ApiErrorResponse
                {
                    code = notFoundException.Code,
                    message = notFoundException.Message
                };
                break;
            case AppException appException:
                statusCode = appException.Code switch
                {
                    ErrorCode.AuthInvalidCredentials => StatusCodes.Status401Unauthorized,
                    ErrorCode.AuthUnauthorized => StatusCodes.Status401Unauthorized,
                    ErrorCode.AuthEmailNotVerified => StatusCodes.Status403Forbidden,
                    ErrorCode.AuthAccountInactive => StatusCodes.Status403Forbidden,
                    ErrorCode.AuthGoogleTokenInvalid => StatusCodes.Status401Unauthorized,
                    ErrorCode.AuthOtpRateLimited => StatusCodes.Status429TooManyRequests,
                    ErrorCode.AuthRateLimited => StatusCodes.Status429TooManyRequests,
                    ErrorCode.AuthRefreshTokenInvalid => StatusCodes.Status401Unauthorized,
                    ErrorCode.AuthRefreshTokenExpired => StatusCodes.Status401Unauthorized,
                    ErrorCode.AuthRefreshTokenRevoked => StatusCodes.Status401Unauthorized,
                    ErrorCode.AuthRefreshTokenReuseDetected => StatusCodes.Status401Unauthorized,
                    _ => StatusCodes.Status400BadRequest
                };
                var localizer = context.RequestServices.GetRequiredService<IErrorMessageLocalizer>();
                error = BuildAppExceptionResponse(appException, localizer);
                break;
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(error);
    }

    private static ApiErrorResponse BuildAppExceptionResponse(
        AppException appException,
        IErrorMessageLocalizer localizer)
    {
        if (!ErrorCode.IsTourModuleError(appException.Code))
        {
            return new ApiErrorResponse
            {
                code = appException.Code,
                message = appException.Message,
                fieldErrors = appException.FieldErrors
            };
        }

        return new ApiErrorResponse
        {
            code = appException.Code,
            message = localizer.Localize(appException.Message),
            fieldErrors = localizer.LocalizeFieldErrors(appException.FieldErrors)
        };
    }
}