namespace Workplan.Infrastructure.Messaging.Webhooks;

public sealed class WebhookOptions
{
    public const string SectionName = "IntegrationWebhook";

    public bool Enabled { get; set; }
    public string? Endpoint { get; set; }
    public string? SigningSecret { get; set; }
    public int TimeoutSeconds { get; set; } = 15;
}
