namespace Workplan.Infrastructure.Messaging.Outbox;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public int PollIntervalSeconds { get; set; } = 2;
    public int BatchSize { get; set; } = 20;
    public int MaxRetryCount { get; set; } = 8;
    public int BaseRetryDelaySeconds { get; set; } = 5;
    public int MaxRetryDelaySeconds { get; set; } = 300;
}
