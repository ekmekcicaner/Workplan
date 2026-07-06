using System.Net.Http.Json;
using Workplan.Client.Json;
using Workplan.Client.Models;

namespace Workplan.Client.Services;

public class WorkItemTypesApiService(HttpClient http)
{
    public async Task<ApiResult<Guid>> CreateAsync(CreateWorkItemTypeRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("/api/work-item-types/", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync<Guid>(ct);
    }

    public async Task<ApiResult<List<WorkItemTypeDto>>> GetTreeAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"/api/work-item-types/tree?includeInactive={includeInactive}", ct);
        return await response.ToApiResultAsync<List<WorkItemTypeDto>>(ct);
    }

    public async Task<ApiResult<bool>> UpdateAsync(Guid id, UpdateWorkItemTypeRequest request, CancellationToken ct = default)
    {
        var response = await http.PutAsJsonAsync($"/api/work-item-types/{id}", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }

    public async Task<ApiResult<bool>> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/work-item-types/{id}/activation", new SetActiveRequest(isActive), AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }
}
