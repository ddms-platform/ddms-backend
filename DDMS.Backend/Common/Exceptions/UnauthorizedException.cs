using DDMS.Backend.Common.Constants;

namespace DDMS.Backend.Common.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = ErrorDefinitions.Messages.Unauthorized)
        : base(ErrorDefinitions.Codes.AuthUnauthorized, message)
    {
    }
}
