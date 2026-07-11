using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Workplan.WebApi.Tests.TestInfrastructure;
using Xunit;

namespace Workplan.WebApi.Tests;

public class EndpointContractTests : IClassFixture<WorkplanApiFactory>
{
    private readonly WorkplanApiFactory _factory;

    public EndpointContractTests(WorkplanApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    [Trait("Category", "WebApi")]
    public async Task Protected_endpoint_returns_unauthorized_api_response_for_anonymous_user()
    {
        if (!DockerTestGuard.Enabled)
            return;

        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/projects/");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":false");
        body.Should().Contain("unauthorized");
    }

    [Fact]
    [Trait("Category", "WebApi")]
    public async Task Unknown_route_returns_json_not_found_contract()
    {
        if (!DockerTestGuard.Enabled)
            return;

        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/does-not-exist");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":false");
        body.Should().Contain("not_found");
    }

    [Fact]
    [Trait("Category", "WebApi")]
    public void Api_routes_keep_single_api_prefix_and_expected_resource_groups()
    {
        if (!DockerTestGuard.Enabled)
            return;

        using var client = _factory.CreateClient();
        var routes = _factory.Services.GetRequiredService<EndpointDataSource>()
            .Endpoints
            .OfType<RouteEndpoint>()
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .OfType<string>()
            .ToList();

        routes.Should().NotContain(route => route.Contains("/api/api", StringComparison.Ordinal));
        routes.Should().Contain(route => route.StartsWith("/api/auth", StringComparison.Ordinal));
        routes.Should().Contain(route => route.StartsWith("/api/projects", StringComparison.Ordinal));
        routes.Should().Contain(route => route.StartsWith("/api/crew-regions", StringComparison.Ordinal));
        routes.Should().Contain(route => route.StartsWith("/api/locations", StringComparison.Ordinal));
        routes.Should().Contain(route => route.StartsWith("/api/work-item-types", StringComparison.Ordinal));
        routes.Should().Contain(route => route.StartsWith("/api/crew-types", StringComparison.Ordinal));
        routes.Should().Contain(route => route == "/api/daily-plans/tracking");
        routes.Should().Contain(route => route == "/api/reports/daily");
        routes.Should().Contain(route => route.StartsWith("/api/notifications", StringComparison.Ordinal));
        routes.Should().Contain(route => route.StartsWith("/api/users", StringComparison.Ordinal));
    }

    [Fact]
    [Trait("Category", "WebApi")]
    public async Task Api_rate_limit_returns_standard_429_and_does_not_limit_health_endpoint()
    {
        if (!DockerTestGuard.Enabled)
            return;

        using var rateLimitedFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("RateLimiting:TokenLimit", "2");
            builder.UseSetting("RateLimiting:TokensPerPeriod", "1");
            builder.UseSetting("RateLimiting:ReplenishmentPeriodSeconds", "60");
            builder.UseSetting("RateLimiting:QueueLimit", "0");
        });
        using var client = rateLimitedFactory.CreateClient();

        (await client.GetAsync("/api/projects/")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await client.GetAsync("/api/projects/")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var rejected = await client.GetAsync("/api/projects/");

        rejected.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        var body = await rejected.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":false");
        body.Should().Contain("rate_limited");

        var health = await client.GetAsync("/health/live");
        health.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
