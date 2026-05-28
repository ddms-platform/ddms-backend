using DDMS.Backend.Common.Constants;

namespace DDMS.Backend.Common.Exceptions;

public class ValidationException : AppException
{
    public ValidationException(string message, Dictionary<string, List<string>>? fieldErrors = null)
        : base(ErrorCodes.AuthValidationFailed, message, fieldErrors)
    {
    }
}
