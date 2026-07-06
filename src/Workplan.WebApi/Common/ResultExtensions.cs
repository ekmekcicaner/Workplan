using Workplan.SharedKernel.Common;

namespace Workplan.WebApi.Common;

public static class ResultExtensions
{
    // TODO: bu switch büyüdükçe error code -> status code mapping'i bir yere taşınmalı (Error içine falan)

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
        var statusCode = error.Code switch
        {
            "not_found" => StatusCodes.Status404NotFound,
            "unauthorized" => StatusCodes.Status401Unauthorized,
            "scope_mismatch" => StatusCodes.Status403Forbidden,
            "out_of_order" => StatusCodes.Status409Conflict,
            "Validation" => StatusCodes.Status422UnprocessableEntity,
            "sentinel" => StatusCodes.Status422UnprocessableEntity,
            "empty_submit" => StatusCodes.Status422UnprocessableEntity,
            "terminal_immutable" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Json(
            ApiResponse<object?>.CreateFailure(ApiError.FromError(error)),
            statusCode: statusCode);
    }
}
