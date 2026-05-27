namespace DDMS.Backend.Common.Responses;

public class ApiResponse<T>
{
    public bool success { get; set; }
    public string message { get; set; } = string.Empty;
    public T? data { get; set; }

    public static ApiResponse<T> Ok(T? data, string message)
    {
        return new ApiResponse<T>
        {
            success = true,
            message = message,
            data = data
        };
    }
}

public class ApiErrorResponse
{
    public bool success { get; set; }
    public string message { get; set; } = string.Empty;
    public object? data { get; set; }

    public static ApiErrorResponse Fail(string message, object? data = null)
    {
        return new ApiErrorResponse
        {
            success = false,
            message = message,
            data = data
        };
    }
}
