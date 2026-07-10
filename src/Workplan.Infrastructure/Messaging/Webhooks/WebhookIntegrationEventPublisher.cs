using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Workplan.Application.Common.Messaging;
using Workplan.Infrastructure.Messaging.Outbox;

namespace Workplan.Infrastructure.Messaging.Webhooks;

public sealed class WebhookIntegrationEventPublisher(
    HttpClient httpClient,
    IOptions<WebhookOptions> options)
{
    public const string SignatureHeaderName = "X-Workplan-Signature";
    public const string EventIdHeaderName = "X-Workplan-Event-Id";
    public const string EventTypeHeaderName = "X-Workplan-Event-Type";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly WebhookOptions _options = options.Value;

    public async Task PublishAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
            return;

        var eventType = IntegrationEventSerializer.GetEventName(integrationEvent);
        var data = JsonSerializer.SerializeToElement(
            integrationEvent,
            integrationEvent.GetType(),
            JsonOptions);
        var envelope = new WebhookEnvelope(
            integrationEvent.EventId,
            eventType,
            integrationEvent.OccurredOnUtc,
            data);
        var body = JsonSerializer.SerializeToUtf8Bytes(envelope, JsonOptions);

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint)
        {
            Content = new ByteArrayContent(body)
        };
        request.Content.Headers.ContentType = new("application/json");
        request.Headers.Add(EventIdHeaderName, integrationEvent.EventId.ToString("D"));
        request.Headers.Add(EventTypeHeaderName, eventType);
        request.Headers.Add(SignatureHeaderName, CreateSignature(body, _options.SigningSecret!));

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        using var response = await httpClient.SendAsync(request, timeout.Token);
        response.EnsureSuccessStatusCode();
    }

    public static string CreateSignature(ReadOnlySpan<byte> body, string secret)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var hash = HMACSHA256.HashData(key, body);
        return $"sha256={Convert.ToHexStringLower(hash)}";
    }

    private sealed record WebhookEnvelope(
        Guid Id,
        string Type,
        DateTime OccurredOnUtc,
        JsonElement Data);
}
