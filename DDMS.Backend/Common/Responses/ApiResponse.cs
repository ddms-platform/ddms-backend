using DDMS.Backend.Common.Constants;

namespace DDMS.Backend.Common.Responses;

public class ApiResponse<T>
{
    public int code { get; init; }
    public T result { get; init; } = default!;

    public static ApiResponse<T> Ok(T result) =>
        new()
        {
            code = ErrorDefinitions.Codes.Success,
            result = result
        };
}
