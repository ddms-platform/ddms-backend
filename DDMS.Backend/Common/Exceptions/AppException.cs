namespace DDMS.Backend.Common.Exceptions;

public class AppException : Exception
{
    public int Code { get; }
    public Dictionary<string, List<string>>? FieldErrors { get; }

    public AppException(int code, string message, Dictionary<string, List<string>>? fieldErrors = null)
        : base(message)
    {
        Code = code;
        FieldErrors = fieldErrors;
    }
}

public class ValidationException : AppException
{
    public ValidationException(string message, Dictionary<string, List<string>>? fieldErrors = null)
        : base(ErrorCode.AuthValidationFailed, message, fieldErrors)
    {
    }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = ErrorCode.Messages.Unauthorized)
        : base(ErrorCode.AuthUnauthorized, message)
    {
    }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = ErrorCode.Messages.Forbidden)
        : base(ErrorCode.Forbidden, message)
    {
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message = ErrorCode.Messages.ResourceNotFound)
        : base(ErrorCode.ResourceNotFound, message)
    {
    }

    public NotFoundException(int code, string message)
        : base(code, message)
    {
    }
}
