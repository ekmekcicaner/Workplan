using System.Net.Http.Json;
using Workplan.Client.Json;
using Workplan.Client.Models;

namespace Workplan.Client.Services;

public class ProjectsApiService(HttpClient http)
{
    public async Task<ApiResult<Guid>> CreateAsync(CreateProjectRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("/api/projects/", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync<Guid>(ct);
    }

    public async Task<ApiResult<List<ProjectDto>>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"/api/projects/?includeInactive={includeInactive}", ct);
        return await response.ToApiResultAsync<List<ProjectDto>>(ct);
    }

    public async Task<ApiResult<ProjectDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"/api/projects/{id}", ct);
        return await response.ToApiResultAsync<ProjectDto>(ct);
    }

    public async Task<ApiResult<bool>> UpdateAsync(Guid id, UpdateProjectRequest request, CancellationToken ct = default)
    {
        var response = await http.PutAsJsonAsync($"/api/projects/{id}", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }

    public async Task<ApiResult<bool>> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/projects/{id}/activation", new SetActiveRequest(isActive), AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }
}
