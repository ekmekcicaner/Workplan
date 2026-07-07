using System.Net;
using FluentAssertions;
using Workplan.Client.Models;
using Workplan.Client.Services;
using Xunit;

namespace Workplan.Client.Tests;

public class DailyPlansApiServiceTests
{
    [Fact]
    [Trait("Category", "Client")]
    public async Task CreateAsync_posts_expected_endpoint_and_maps_success_id()
    {
        var expectedId = Guid.NewGuid();
        var handler = new FakeHttpMessageHandler(_ =>
            FakeHttpMessageHandler.Json(
                HttpStatusCode.OK,
                $$"""{"success":true,"data":"{{expectedId}}","error":null}"""));
        var service = CreateService(handler);
        var request = new CreateDailyPlanRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            10,
            2,
            Guid.NewGuid());

        var result = await service.CreateAsync(request);

        result.Success.Should().BeTrue();
        result.Data.Should().Be(expectedId);
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        handler.LastRequest.RequestUri!.PathAndQuery.Should().Be("/api/daily-plans/");
        var body = await handler.LastRequest.Content!.ReadAsStringAsync();
        body.Should().Contain("\"plannedQuantity\":10");
    }

    [Fact]
    [Trait("Category", "Client")]
    public async Task SubmitProgressAsync_maps_api_error_message()
    {
        var handler = new FakeHttpMessageHandler(_ =>
            FakeHttpMessageHandler.Json(
                HttpStatusCode.UnprocessableEntity,
                """{"success":false,"data":null,"error":{"code":"Validation","message":"Invalid progress","details":null}}"""));
        var service = CreateService(handler);

        var result = await service.SubmitProgressAsync(
            Guid.NewGuid(),
            new SubmitProgressRequest(5, null, null, null));

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid progress");
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        handler.LastRequest.RequestUri!.PathAndQuery.Should().Contain("/submit-progress");
    }

    [Fact]
    [Trait("Category", "Client")]
    public async Task StartAsync_posts_crew_type_id_without_crew_id()
    {
        var planId = Guid.NewGuid();
        var crewTypeId = Guid.NewGuid();
        var handler = new FakeHttpMessageHandler(_ =>
            FakeHttpMessageHandler.Json(
                HttpStatusCode.OK,
                """{"success":true,"data":null,"error":null}"""));
        var service = CreateService(handler);

        var result = await service.StartAsync(planId, crewTypeId);

        result.Success.Should().BeTrue();
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        handler.LastRequest.RequestUri!.PathAndQuery.Should().Be($"/api/daily-plans/{planId}/start");
        var body = await handler.LastRequest.Content!.ReadAsStringAsync();
        body.Should().Contain($"\"crewTypeId\":\"{crewTypeId}\"");
        body.Should().NotContain("crewId");
    }

    [Fact]
    [Trait("Category", "Client")]
    public async Task New_read_endpoints_call_expected_paths()
    {
        var planId = Guid.NewGuid();
        var handler = new FakeHttpMessageHandler(request =>
        {
            var json = request.RequestUri!.PathAndQuery switch
            {
                "/api/daily-plans/my-work" => """{"success":true,"data":[],"error":null}""",
                "/api/daily-plans/approval-queue" => """{"success":true,"data":[],"error":null}""",
                var path when path == $"/api/daily-plans/{planId}/detail" =>
                    $$"""{"success":true,"data":{"id":"{{planId}}","projectId":"{{Guid.NewGuid()}}","projectName":"P","crewRegionId":"{{Guid.NewGuid()}}","crewRegionName":"R","locationId":"{{Guid.NewGuid()}}","locationName":"L","workItemTypeId":"{{Guid.NewGuid()}}","workItemTypeName":"W","workDate":"2026-07-06","plannedById":"{{Guid.NewGuid()}}","assignedHoMId":null,"assignedHoMName":"Mustafa Arslan","siteChiefName":"Ayşe Demir","projectManagerName":"Mehmet Kaya","crewTypeId":null,"crewTypeName":null,"plannedQuantity":1,"plannedManDay":1,"unit":"M2","factQuantity":null,"factManDay":null,"overtime":null,"comment":null,"status":"Assigned","history":[{"fromStatus":"Draft","toStatus":"Assigned","actionById":"{{Guid.NewGuid()}}","transitionedAt":"2026-07-06T07:00:00Z","actionByName":"Teknik Ofis","note":"Atandı"}]},"error":null}""",
                _ => """{"success":false,"data":null,"error":{"code":"not_found","message":"not found","details":null}}"""
            };

            return FakeHttpMessageHandler.Json(HttpStatusCode.OK, json);
        });
        var service = CreateService(handler);

        (await service.GetMyWorkAsync()).Success.Should().BeTrue();
        handler.LastRequest!.RequestUri!.PathAndQuery.Should().Be("/api/daily-plans/my-work");

        (await service.GetApprovalQueueAsync()).Success.Should().BeTrue();
        handler.LastRequest!.RequestUri!.PathAndQuery.Should().Be("/api/daily-plans/approval-queue");

        var detail = await service.GetDetailAsync(planId);
        detail.Success.Should().BeTrue();
        detail.Data!.AssignedHoMName.Should().Be("Mustafa Arslan");
        detail.Data.SiteChiefName.Should().Be("Ayşe Demir");
        detail.Data.ProjectManagerName.Should().Be("Mehmet Kaya");
        detail.Data.History.Should().ContainSingle().Which.ActionByName.Should().Be("Teknik Ofis");
        handler.LastRequest!.RequestUri!.PathAndQuery.Should().Be($"/api/daily-plans/{planId}/detail");
    }

    [Fact]
    [Trait("Category", "Client")]
    public async Task Tracking_endpoints_call_expected_paths_and_query_values()
    {
        var projectId = Guid.NewGuid();
        var regionId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var homId = Guid.NewGuid();
        var siteChiefId = Guid.NewGuid();
        var pmId = Guid.NewGuid();
        var handler = new FakeHttpMessageHandler(request =>
        {
            var json = request.RequestUri!.PathAndQuery.StartsWith("/api/daily-plans/tracking/options", StringComparison.Ordinal)
                ? """{"success":true,"data":{"projects":[],"crewRegions":[],"locations":[],"headOfMasters":[],"siteChiefs":[],"projectManagers":[],"statuses":["Assigned"]},"error":null}"""
                : """{"success":true,"data":[],"error":null}""";

            return FakeHttpMessageHandler.Json(HttpStatusCode.OK, json);
        });
        var service = CreateService(handler);

        (await service.GetTrackingOptionsAsync()).Success.Should().BeTrue();
        handler.LastRequest!.RequestUri!.PathAndQuery.Should().Be("/api/daily-plans/tracking/options");

        var result = await service.GetTrackingAsync(
            new DateOnly(2026, 7, 7),
            [projectId],
            [regionId],
            [locationId],
            [homId],
            [siteChiefId],
            [pmId],
            [WorkStatus.Assigned, WorkStatus.InProgress]);

        result.Success.Should().BeTrue();
        var pathAndQuery = handler.LastRequest!.RequestUri!.PathAndQuery;
        pathAndQuery.Should().StartWith("/api/daily-plans/tracking?");
        pathAndQuery.Should().Contain("workDate=2026-07-07");
        pathAndQuery.Should().Contain($"projectIds={projectId}");
        pathAndQuery.Should().Contain($"crewRegionIds={regionId}");
        pathAndQuery.Should().Contain($"locationIds={locationId}");
        pathAndQuery.Should().Contain($"headOfMasterIds={homId}");
        pathAndQuery.Should().Contain($"siteChiefIds={siteChiefId}");
        pathAndQuery.Should().Contain($"projectManagerIds={pmId}");
        pathAndQuery.Should().Contain("statuses=Assigned");
        pathAndQuery.Should().Contain("statuses=InProgress");
    }

    private static DailyPlansApiService CreateService(FakeHttpMessageHandler handler)
    {
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://workplan.test")
        };

        return new DailyPlansApiService(http);
    }
}
