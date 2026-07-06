using Workplan.Client.Models;

namespace Workplan.Client.Services;

public class NotificationsApiService(HttpClient http)
{
    public async Task<ApiResult<List<NotificationDto>>> GetUnreadAsync(CancellationToken ct = default)
    {
        var response = await http.GetAsync("/api/notifications/unread", ct);
        return await response.ToApiResultAsync<List<NotificationDto>>(ct);
    }

    public async Task<ApiResult<bool>> MarkReadAsync(Guid id, CancellationToken ct = default)
    {
        var response = await http.PostAsync($"/api/notifications/{id}/read", content: null, ct);
        return await response.ToApiResultAsync(ct);
    }

    public async Task<ApiResult<bool>> MarkDailyPlanReadAsync(Guid dailyPlanId, CancellationToken ct = default)
    {
        var response = await http.PostAsync($"/api/notifications/daily-plan/{dailyPlanId}/read", content: null, ct);
        return await response.ToApiResultAsync(ct);
    }
}
