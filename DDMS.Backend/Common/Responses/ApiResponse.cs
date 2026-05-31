using DDMS.Backend.Common.Exceptions;

namespace DDMS.Backend.Common.Responses;

public class ApiResponse<T>
{
    public int code { get; init; }
    public T result { get; init; } = default!;

    public static ApiResponse<T> Ok(T result) =>
        new()
        {
            code = ErrorCode.Success,
            result = result
        };
}
