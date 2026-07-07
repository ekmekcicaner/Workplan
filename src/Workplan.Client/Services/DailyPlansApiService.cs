using System.Net.Http.Json;
using Workplan.Client.Json;
using Workplan.Client.Models;

namespace Workplan.Client.Services;

public class DailyPlansApiService(HttpClient http)
{
    public async Task<ApiResult<Guid>> CreateAsync(CreateDailyPlanRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("/api/daily-plans/", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync<Guid>(ct);
    }

    public async Task<ApiResult<bool>> StartAsync(Guid id, Guid crewTypeId, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync($"/api/daily-plans/{id}/start", new StartWorkRequest(crewTypeId), AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }

    public async Task<ApiResult<bool>> SubmitProgressAsync(Guid id, SubmitProgressRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync($"/api/daily-plans/{id}/submit-progress", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }

    public async Task<ApiResult<bool>> ApproveAsync(Guid id, CancellationToken ct = default)
    {
        var response = await http.PostAsync($"/api/daily-plans/{id}/approve", content: null, ct);
        return await response.ToApiResultAsync(ct);
    }

    public async Task<ApiResult<bool>> RejectAsync(Guid id, RejectRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync($"/api/daily-plans/{id}/reject", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }

    public async Task<ApiResult<List<DailyPlanDto>>> GetByHeadOfMasterAsync(Guid headOfMasterUserId, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"/api/daily-plans/by-head-of-master/{headOfMasterUserId}", ct);
        return await response.ToApiResultAsync<List<DailyPlanDto>>(ct);
    }

    public async Task<ApiResult<List<DailyPlanListItemDto>>> GetMyWorkAsync(CancellationToken ct = default)
    {
        var response = await http.GetAsync("/api/daily-plans/my-work", ct);
        return await response.ToApiResultAsync<List<DailyPlanListItemDto>>(ct);
    }

    public async Task<ApiResult<List<DailyPlanListItemDto>>> GetApprovalQueueAsync(CancellationToken ct = default)
    {
        var response = await http.GetAsync("/api/daily-plans/approval-queue", ct);
        return await response.ToApiResultAsync<List<DailyPlanListItemDto>>(ct);
    }

    public async Task<ApiResult<DailyTrackingOptionsDto>> GetTrackingOptionsAsync(CancellationToken ct = default)
    {
        var response = await http.GetAsync("/api/daily-plans/tracking/options", ct);
        return await response.ToApiResultAsync<DailyTrackingOptionsDto>(ct);
    }

    public async Task<ApiResult<List<DailyTrackingPlanDto>>> GetTrackingAsync(
        DateOnly workDate,
        IReadOnlyCollection<Guid>? projectIds = null,
        IReadOnlyCollection<Guid>? crewRegionIds = null,
        IReadOnlyCollection<Guid>? locationIds = null,
        IReadOnlyCollection<Guid>? headOfMasterIds = null,
        IReadOnlyCollection<Guid>? siteChiefIds = null,
        IReadOnlyCollection<Guid>? projectManagerIds = null,
        IReadOnlyCollection<WorkStatus>? statuses = null,
        CancellationToken ct = default)
    {
        var query = new List<string> { $"workDate={workDate:yyyy-MM-dd}" };
        AddGuidValues(query, "projectIds", projectIds);
        AddGuidValues(query, "crewRegionIds", crewRegionIds);
        AddGuidValues(query, "locationIds", locationIds);
        AddGuidValues(query, "headOfMasterIds", headOfMasterIds);
        AddGuidValues(query, "siteChiefIds", siteChiefIds);
        AddGuidValues(query, "projectManagerIds", projectManagerIds);
        AddStatusValues(query, statuses);

        var response = await http.GetAsync($"/api/daily-plans/tracking?{string.Join("&", query)}", ct);
        return await response.ToApiResultAsync<List<DailyTrackingPlanDto>>(ct);
    }

    public async Task<ApiResult<List<DailyPlanDto>>> GetApprovedAsync(CancellationToken ct = default)
    {
        var response = await http.GetAsync("/api/daily-plans/approved", ct);
        return await response.ToApiResultAsync<List<DailyPlanDto>>(ct);
    }

    public async Task<ApiResult<List<DailyPlanDto>>> GetAwaitingApprovalAsync(CancellationToken ct = default)
    {
        var response = await http.GetAsync("/api/daily-plans/awaiting-approval", ct);
        return await response.ToApiResultAsync<List<DailyPlanDto>>(ct);
    }

    public async Task<ApiResult<DailyPlanDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"/api/daily-plans/{id}", ct);
        return await response.ToApiResultAsync<DailyPlanDto>(ct);
    }

    public async Task<ApiResult<DailyPlanDetailDto>> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"/api/daily-plans/{id}/detail", ct);
        return await response.ToApiResultAsync<DailyPlanDetailDto>(ct);
    }

    private static void AddGuidValues(List<string> query, string name, IReadOnlyCollection<Guid>? values)
    {
        if (values is null) return;
        query.AddRange(values.Select(value => $"{name}={value}"));
    }

    private static void AddStatusValues(List<string> query, IReadOnlyCollection<WorkStatus>? statuses)
    {
        if (statuses is null) return;
        query.AddRange(statuses.Select(status => $"statuses={Uri.EscapeDataString(status.ToString())}"));
    }
}
