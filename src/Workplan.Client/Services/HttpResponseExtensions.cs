using System.Text.Json;
using Workplan.Client.Json;
using Workplan.Client.Models;

namespace Workplan.Client.Services;

internal static class HttpResponseExtensions
{
    public static async Task<ApiResult<T>> ToApiResultAsync<T>(this HttpResponseMessage response, CancellationToken ct = default)
    {
        var json = await response.Content.ReadAsStringAsync(ct);

        try
        {
            var body = JsonSerializer.Deserialize<ApiResponse<T>>(json, AppJsonOptions.Default);
            if (body is { Success: true })
                return ApiResult<T>.Ok(body.Data);

            return ApiResult<T>.Fail(body?.Error?.Message ?? ExtractErrorMessage(response, json));
        }
        catch (JsonException)
        {
            return ApiResult<T>.Fail(ExtractErrorMessage(response, json));
        }
    }

    public static async Task<ApiResult<bool>> ToApiResultAsync(this HttpResponseMessage response, CancellationToken ct = default)
    {
        var json = await response.Content.ReadAsStringAsync(ct);

        try
        {
            var body = JsonSerializer.Deserialize<ApiResponse<object?>>(json, AppJsonOptions.Default);
            if (body is { Success: true })
                return ApiResult<bool>.Ok(true);

            return ApiResult<bool>.Fail(body?.Error?.Message ?? ExtractErrorMessage(response, json));
        }
        catch (JsonException)
        {
            return ApiResult<bool>.Fail(ExtractErrorMessage(response, json));
        }
    }

    private static string ExtractErrorMessage(HttpResponseMessage response, string json)
    {
        if (LooksLikeHtml(response, json))
        {
            var uri = response.RequestMessage?.RequestUri?.ToString() ?? "bilinmeyen endpoint";
            return $"API JSON yerine HTML döndürdü ({(int)response.StatusCode} {response.ReasonPhrase}) - {uri}";
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("error", out var errorEl) &&
                errorEl.TryGetProperty("message", out var messageEl))
            {
                return messageEl.GetString() ?? "Beklenmeyen bir hata oluştu.";
            }
        }
        catch
        {
        }

        return "Beklenmeyen bir hata oluştu.";
    }

    private static bool LooksLikeHtml(HttpResponseMessage response, string body)
    {
        var contentType = response.Content.Headers.ContentType?.MediaType;
        if (contentType?.Contains("html", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        var trimmed = body.TrimStart();
        return trimmed.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) ||
               trimmed.StartsWith("<html", StringComparison.OrdinalIgnoreCase);
    }
}
