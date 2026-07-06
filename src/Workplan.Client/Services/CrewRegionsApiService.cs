using System.Net.Http.Json;
using Workplan.Client.Json;
using Workplan.Client.Models;

namespace Workplan.Client.Services;

public class CrewRegionsApiService(HttpClient http)
{
    public async Task<ApiResult<Guid>> CreateAsync(CreateCrewRegionRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("/api/crew-regions/", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync<Guid>(ct);
    }

    public async Task<ApiResult<List<CrewRegionDto>>> GetByProjectAsync(
        Guid projectId, bool includeInactive = false, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"/api/crew-regions/by-project/{projectId}?includeInactive={includeInactive}", ct);
        return await response.ToApiResultAsync<List<CrewRegionDto>>(ct);
    }

    public async Task<ApiResult<CrewRegionDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"/api/crew-regions/{id}", ct);
        return await response.ToApiResultAsync<CrewRegionDto>(ct);
    }

    public async Task<ApiResult<bool>> UpdateAsync(Guid id, UpdateCrewRegionRequest request, CancellationToken ct = default)
    {
        var response = await http.PutAsJsonAsync($"/api/crew-regions/{id}", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }

    public async Task<ApiResult<bool>> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/crew-regions/{id}/activation", new SetActiveRequest(isActive), AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }

    public async Task<ApiResult<bool>> AssignSiteChiefAsync(Guid id, Guid siteChiefUserId, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/crew-regions/{id}/assign-site-chief", new AssignSiteChiefRequest(siteChiefUserId), AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }

    public async Task<ApiResult<bool>> AssignTechOfficeAsync(Guid id, Guid techOfficeUserId, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/crew-regions/{id}/assign-tech-office", new AssignTechOfficeRequest(techOfficeUserId), AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }
}
