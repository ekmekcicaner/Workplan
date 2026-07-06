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

    public async Task<ApiResult<bool>> StartAsync(Guid id, Guid crewId, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync($"/api/daily-plans/{id}/start", new StartWorkRequest(crewId), AppJsonOptions.Default, ct);
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
}
