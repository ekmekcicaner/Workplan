namespace Workplan.Client.Models;

public record ApiResult<T>(bool Success, T? Data, string? ErrorMessage)
{
    public static ApiResult<T> Ok(T? data) => new(true, data, null);
    public static ApiResult<T> Fail(string errorMessage) => new(false, default, errorMessage);
}
