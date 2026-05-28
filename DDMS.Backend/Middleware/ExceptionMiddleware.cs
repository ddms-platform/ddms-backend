using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;

namespace DDMS.Backend.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = StatusCodes.Status500InternalServerError;
        var error = new ApiErrorResponse
        {
            code = ErrorCodes.UncategorizedError,
            message = MessageConstants.UncategorizedError
        };

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = StatusCodes.Status400BadRequest;
                error = new ApiErrorResponse
                {
                    code = validationException.ErrorCode,
                    message = validationException.Message,
                    fieldErrors = validationException.FieldErrors
                };
                break;
            case UnauthorizedException unauthorizedException:
                statusCode = StatusCodes.Status401Unauthorized;
                error = new ApiErrorResponse
                {
                    code = unauthorizedException.ErrorCode,
                    message = unauthorizedException.Message
                };
                break;
            case ForbiddenException forbiddenException:
                statusCode = StatusCodes.Status403Forbidden;
                error = new ApiErrorResponse
                {
                    code = forbiddenException.ErrorCode,
                    message = forbiddenException.Message
                };
                break;
            case NotFoundException notFoundException:
                statusCode = StatusCodes.Status404NotFound;
                error = new ApiErrorResponse
                {
                    code = notFoundException.ErrorCode,
                    message = notFoundException.Message
                };
                break;
            case AppException appException:
                statusCode = appException.ErrorCode switch
                {
                    ErrorCodes.AuthInvalidCredentials => StatusCodes.Status401Unauthorized,
                    ErrorCodes.AuthUnauthorized => StatusCodes.Status401Unauthorized,
                    ErrorCodes.AuthEmailNotVerified => StatusCodes.Status403Forbidden,
                    ErrorCodes.AuthGoogleTokenInvalid => StatusCodes.Status401Unauthorized,
                    ErrorCodes.AuthOtpRateLimited => StatusCodes.Status429TooManyRequests,
                    _ => StatusCodes.Status400BadRequest
                };
                error = new ApiErrorResponse
                {
                    code = appException.ErrorCode,
                    message = appException.Message,
                    fieldErrors = appException.FieldErrors
                };
                break;
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(error);
    }
}
