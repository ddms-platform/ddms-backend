using DDMS.Backend.Common.Constants;

namespace DDMS.Backend.Common.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = MessageConstants.Forbidden)
        : base(ErrorCodes.Forbidden, message)
    {
    }
}
