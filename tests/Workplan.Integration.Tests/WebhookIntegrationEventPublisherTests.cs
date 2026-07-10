using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Workplan.Application.Features.DailyPlans.IntegrationEvents;
using Workplan.Infrastructure.Messaging.Webhooks;
using Xunit;

namespace Workplan.Integration.Tests;

public class WebhookIntegrationEventPublisherTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Publish_sends_event_envelope_headers_and_hmac_signature()
    {
        const string signingSecret = "integration-test-secret";
        var endpoint = new Uri("https://hooks.example.test/workplan");
        var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler);
        var publisher = new WebhookIntegrationEventPublisher(
            httpClient,
            Options.Create(new WebhookOptions
            {
                Enabled = true,
                Endpoint = endpoint.ToString(),
                SigningSecret = signingSecret,
                TimeoutSeconds = 10
            }));
        var integrationEvent = new DailyPlanFullyApproved(
            Guid.Parse("49bd355d-b70e-4ff5-a62d-eecb127c6aaa"),
            new DateTime(2026, 7, 10, 9, 30, 0, DateTimeKind.Utc),
            Guid.Parse("c02bcd77-20b4-4c3c-8262-339ac7604e56"));

        await publisher.PublishAsync(integrationEvent, CancellationToken.None);

        handler.Method.Should().Be(HttpMethod.Post);
        handler.RequestUri.Should().Be(endpoint);
        handler.ContentType.Should().Be("application/json");
        handler.Headers[WebhookIntegrationEventPublisher.EventIdHeaderName]
            .Should().Equal(integrationEvent.EventId.ToString("D"));
        handler.Headers[WebhookIntegrationEventPublisher.EventTypeHeaderName]
            .Should().Equal(DailyPlanFullyApproved.EventName);

        var expectedHash = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(signingSecret),
            handler.Body);
        handler.Headers[WebhookIntegrationEventPublisher.SignatureHeaderName]
            .Should().Equal($"sha256={Convert.ToHexStringLower(expectedHash)}");

        using var body = JsonDocument.Parse(handler.Body);
        body.RootElement.GetProperty("id").GetGuid().Should().Be(integrationEvent.EventId);
        body.RootElement.GetProperty("type").GetString().Should().Be(DailyPlanFullyApproved.EventName);
        body.RootElement.GetProperty("occurredOnUtc").GetDateTime().Should().Be(integrationEvent.OccurredOnUtc);
        var data = body.RootElement.GetProperty("data");
        data.GetProperty("eventId").GetGuid().Should().Be(integrationEvent.EventId);
        data.GetProperty("occurredOnUtc").GetDateTime().Should().Be(integrationEvent.OccurredOnUtc);
        data.GetProperty("dailyPlanId").GetGuid().Should().Be(integrationEvent.DailyPlanId);
    }

    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }
        public Uri? RequestUri { get; private set; }
        public string? ContentType { get; private set; }
        public byte[] Body { get; private set; } = [];
        public Dictionary<string, string[]> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Method = request.Method;
            RequestUri = request.RequestUri;
            ContentType = request.Content?.Headers.ContentType?.MediaType;
            Body = await request.Content!.ReadAsByteArrayAsync(cancellationToken);

            foreach (var header in request.Headers)
                Headers[header.Key] = header.Value.ToArray();

            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }
    }
}
