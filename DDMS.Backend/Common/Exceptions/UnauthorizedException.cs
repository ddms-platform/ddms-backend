using DDMS.Backend.Common.Constants;

namespace DDMS.Backend.Common.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = MessageConstants.Unauthorized)
        : base(ErrorCodes.AuthUnauthorized, message)
    {
    }
}
