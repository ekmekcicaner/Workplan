using System.Net;
using FluentAssertions;
using Workplan.Client.Services;
using Xunit;

namespace Workplan.Client.Tests;

public class ReportsApiServiceTests
{
    [Fact]
    [Trait("Category", "Client")]
    public async Task GetDailyAsync_builds_filter_query()
    {
        var projectId = Guid.NewGuid();
        var regionId = Guid.NewGuid();
        var handler = new FakeHttpMessageHandler(_ =>
            FakeHttpMessageHandler.Json(
                HttpStatusCode.OK,
                """{"success":true,"data":{"summary":{"approvedWorkCount":0,"plannedManDay":0,"factManDay":0,"overtime":0,"manDayRealizationRatio":null},"quantityByUnit":[],"quantityByWorkItem":[],"items":[]},"error":null}"""));
        var service = new ReportsApiService(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://workplan.test")
        });

        var result = await service.GetDailyAsync(
            DateOnly.FromDateTime(new DateTime(2026, 7, 6)),
            projectId,
            regionId);

        result.Success.Should().BeTrue();
        handler.LastRequest!.RequestUri!.PathAndQuery.Should().Contain("/api/reports/daily?");
        handler.LastRequest.RequestUri.PathAndQuery.Should().Contain("workDate=2026-07-06");
        handler.LastRequest.RequestUri.PathAndQuery.Should().Contain($"projectId={projectId}");
        handler.LastRequest.RequestUri.PathAndQuery.Should().Contain($"crewRegionId={regionId}");
    }
}
