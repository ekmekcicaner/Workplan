using System.Net.Http.Json;
using Workplan.Client.Json;
using Workplan.Client.Models;

namespace Workplan.Client.Services;

public class CrewsApiService(HttpClient http)
{
    public async Task<ApiResult<Guid>> CreateAsync(CreateCrewRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("/api/crews/", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync<Guid>(ct);
    }

    public async Task<ApiResult<Guid>> AddMemberAsync(Guid crewId, AddCrewMemberRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync($"/api/crews/{crewId}/members", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync<Guid>(ct);
    }

    public async Task<ApiResult<List<CrewDto>>> GetByLocationAsync(Guid locationId, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"/api/crews/by-location/{locationId}", ct);
        return await response.ToApiResultAsync<List<CrewDto>>(ct);
    }
}
