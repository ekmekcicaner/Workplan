namespace Workplan.SharedKernel.Common;

public class ApiResponse<T>
{
    // Frontend dostu "success" (camelCase serileştirilecek)
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResponse<T> CreateSuccess(T data) => new()
    {
        Success = true,
        Data = data,
        Error = null
    };

    public static ApiResponse<T> CreateFailure(ApiError error) => new()
    {
        Success = false,
        Data = default,
        Error = error
    };
}
