using DDMS.Backend.Common.Constants;

namespace DDMS.Backend.Common.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string message = MessageConstants.ResourceNotFound)
        : base(ErrorCodes.ResourceNotFound, message)
    {
    }
}
