using System.Net;
using FluentAssertions;
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
}
