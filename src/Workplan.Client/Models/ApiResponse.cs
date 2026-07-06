namespace Workplan.Client.Models;

public record ApiResponse<T>(bool Success, T? Data, ApiError? Error);

public record ApiError(string Code, string Message, IEnumerable<string>? Details);
