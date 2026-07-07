using Workplan.Client.Models;

namespace Workplan.Client.Services;

public class ReportsApiService(HttpClient http)
{
    public async Task<ApiResult<DailyReportDto>> GetDailyAsync(
        DateOnly? workDate = null,
        Guid? projectId = null,
        Guid? crewRegionId = null,
        Guid? headOfMasterId = null,
        CancellationToken ct = default)
    {
        var query = new List<string>();
        if (workDate is { } date)
            query.Add($"workDate={date:yyyy-MM-dd}");
        if (projectId is { } project)
            query.Add($"projectId={project}");
        if (crewRegionId is { } region)
            query.Add($"crewRegionId={region}");
        if (headOfMasterId is { } hom)
            query.Add($"headOfMasterId={hom}");

        var suffix = query.Count == 0 ? "" : $"?{string.Join("&", query)}";
        var response = await http.GetAsync($"/api/reports/daily{suffix}", ct);
        return await response.ToApiResultAsync<DailyReportDto>(ct);
    }
}
