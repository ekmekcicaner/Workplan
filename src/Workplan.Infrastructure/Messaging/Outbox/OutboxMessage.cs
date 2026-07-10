namespace Workplan.Infrastructure.Messaging.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime NextAttemptOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public DateTime? PoisonedOnUtc { get; private set; }
    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }

    private OutboxMessage()
    {
    }

    internal static OutboxMessage Create(
        Guid id,
        string type,
        string payload,
        DateTime occurredOnUtc) =>
        new()
        {
            Id = id,
            Type = type,
            Payload = payload,
            OccurredOnUtc = occurredOnUtc,
            NextAttemptOnUtc = occurredOnUtc
        };

    internal void MarkProcessed(DateTime processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        LastError = null;
    }

    internal void RecordFailure(
        DateTime failedOnUtc,
        string error,
        int maxRetryCount,
        TimeSpan retryDelay)
    {
        RetryCount++;
        LastError = error.Length <= 2000 ? error : error[..2000];

        if (RetryCount >= maxRetryCount)
        {
            PoisonedOnUtc = failedOnUtc;
            return;
        }

        NextAttemptOnUtc = failedOnUtc.Add(retryDelay);
    }
}
