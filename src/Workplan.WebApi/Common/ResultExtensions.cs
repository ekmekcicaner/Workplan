using Workplan.SharedKernel.Common;

namespace Workplan.WebApi.Common;

public static class ResultExtensions
{
    public static IResult ToApiResult(this Result result) =>
        result.IsSuccess
            ? Results.Ok(ApiResponse<object?>.CreateSuccess(null))
            : ToProblem(result.Error);

    public static IResult ToApiResult<T>(this Result<T> result) =>
        result.IsSuccess
            ? Results.Ok(ApiResponse<T>.CreateSuccess(result.Value))
            : ToProblem(result.Error);

    private static IResult ToProblem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.ScopeMismatch => StatusCodes.Status403Forbidden,
            ErrorType.OutOfOrder => StatusCodes.Status409Conflict,
            ErrorType.TerminalImmutable => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status422UnprocessableEntity,
            ErrorType.Sentinel => StatusCodes.Status422UnprocessableEntity,
            ErrorType.EmptySubmit => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Json(
            ApiResponse<object?>.CreateFailure(ApiError.FromError(error)),
            statusCode: statusCode);
    }
}
