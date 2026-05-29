using DDMS.Backend.Common.Constants;

namespace DDMS.Backend.Common.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = ErrorDefinitions.Messages.Forbidden)
        : base(ErrorDefinitions.Codes.Forbidden, message)
    {
    }
}
