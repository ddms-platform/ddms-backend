using System.Net;

namespace DDMS.Backend.Common.Exceptions;

public class AppException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public object? ErrorData { get; }

    public AppException(string message, HttpStatusCode statusCode, object? errorData = null) : base(message)
    {
        StatusCode = statusCode;
        ErrorData = errorData;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, HttpStatusCode.NotFound)
    {
    }
}

public class BadRequestException : AppException
{
    public BadRequestException(string message, object? errorData = null)
        : base(message, HttpStatusCode.BadRequest, errorData)
    {
    }
}
