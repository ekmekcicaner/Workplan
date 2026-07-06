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

    private static DailyPlansApiService CreateService(FakeHttpMessageHandler handler)
    {
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://workplan.test")
        };

        return new DailyPlansApiService(http);
    }
}
