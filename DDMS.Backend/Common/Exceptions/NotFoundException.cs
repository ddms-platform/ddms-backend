using DDMS.Backend.Common.Constants;

namespace DDMS.Backend.Common.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string message = ErrorDefinitions.Messages.ResourceNotFound)
        : base(ErrorDefinitions.Codes.ResourceNotFound, message)
    {
    }
}
