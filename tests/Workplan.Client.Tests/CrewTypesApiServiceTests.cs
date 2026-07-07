using System.Net;
using FluentAssertions;
using Workplan.Client.Models;
using Workplan.Client.Services;
using Xunit;

namespace Workplan.Client.Tests;

public class CrewTypesApiServiceTests
{
    [Fact]
    [Trait("Category", "Client")]
    public async Task Create_update_and_activation_call_expected_paths()
    {
        var crewTypeId = Guid.NewGuid();
        var handler = new FakeHttpMessageHandler(_ =>
            FakeHttpMessageHandler.Json(HttpStatusCode.OK, $$"""{"success":true,"data":"{{crewTypeId}}","error":null}"""));
        var service = CreateService(handler);

        var createResult = await service.CreateAsync(new CreateCrewTypeRequest("Kalıpçı"));

        createResult.Success.Should().BeTrue();
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        handler.LastRequest.RequestUri!.PathAndQuery.Should().Be("/api/crew-types/");

        await service.UpdateAsync(crewTypeId, new UpdateCrewTypeRequest("Demirci"));
        handler.LastRequest!.Method.Should().Be(HttpMethod.Put);
        handler.LastRequest.RequestUri!.PathAndQuery.Should().Be($"/api/crew-types/{crewTypeId}");

        await service.SetActiveAsync(crewTypeId, false);
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        handler.LastRequest.RequestUri!.PathAndQuery.Should().Be($"/api/crew-types/{crewTypeId}/activation");
    }

    private static CrewTypesApiService CreateService(FakeHttpMessageHandler handler)
    {
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://workplan.test")
        };

        return new CrewTypesApiService(http);
    }
}
