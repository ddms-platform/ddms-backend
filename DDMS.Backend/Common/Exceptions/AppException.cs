namespace DDMS.Backend.Common.Exceptions;

public class AppException : Exception
{
    public int ErrorCode { get; }
    public Dictionary<string, List<string>>? FieldErrors { get; }

    public AppException(int errorCode, string message, Dictionary<string, List<string>>? fieldErrors = null)
        : base(message)
    {
        ErrorCode = errorCode;
        FieldErrors = fieldErrors;
    }
}
