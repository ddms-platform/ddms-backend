namespace DDMS.Backend.Common.Responses;

public class ApiErrorResponse
{
    public int code { get; init; }
    public string message { get; init; } = string.Empty;
    public Dictionary<string, List<string>>? fieldErrors { get; init; }
}
