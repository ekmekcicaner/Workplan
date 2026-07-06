namespace Workplan.SharedKernel.Common;

public sealed record ApiError(string Code, string Message, IEnumerable<string>? Details = null)
{
    public static ApiError FromError(Error error) => new(error.Code, error.Message, error.Details);
}
