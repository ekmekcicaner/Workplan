using Microsoft.AspNetCore.Diagnostics;
using Workplan.SharedKernel.Common;

namespace Workplan.WebApi.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Uygulama çalışırken beklenmeyen bir hata oluştu: {Message}", exception.Message);

        var (statusCode, code, message) = exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "unauthorized", exception.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "not_found", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "server_error",
                "Sistemde beklenmeyen bir hata oluştu. Lütfen teknik ekiple iletişime geçin.")
        };

        httpContext.Response.StatusCode = statusCode;

        var response = ApiResponse<object?>.CreateFailure(new ApiError(code, message));
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        // true: hata burada ele alındı, pipeline'da ileri gitmesin
        return true;
    }
}