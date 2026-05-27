using System.Net;
using System.Text.Json;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;

namespace DDMS.Backend.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception occurred");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "Internal server error";
        object? data = null;

        if (exception is AppException appException)
        {
            statusCode = appException.StatusCode;
            message = appException.Message;
            data = appException.ErrorData;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiErrorResponse.Fail(message, data);
        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}
