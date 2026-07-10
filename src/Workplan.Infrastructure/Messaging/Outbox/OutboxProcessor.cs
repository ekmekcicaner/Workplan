using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Workplan.Application.Common.Messaging;
using Workplan.Infrastructure.Persistence;

namespace Workplan.Infrastructure.Messaging.Outbox;

public sealed class OutboxProcessor(
    AppDbContext dbContext,
    IIntegrationEventPublisher publisher,
    IOptions<OutboxOptions> options,
    TimeProvider timeProvider,
    ILogger<OutboxProcessor> logger)
{
    private readonly OutboxOptions _options = options.Value;

    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var messages = await dbContext.OutboxMessages
            .Where(message =>
                message.ProcessedOnUtc == null
                && message.PoisonedOnUtc == null
                && message.NextAttemptOnUtc <= now)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var integrationEvent = IntegrationEventSerializer.Deserialize(message);
                await publisher.PublishAsync(integrationEvent, cancellationToken);
                message.MarkProcessed(timeProvider.GetUtcNow().UtcDateTime);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                var failedOnUtc = timeProvider.GetUtcNow().UtcDateTime;
                message.RecordFailure(
                    failedOnUtc,
                    exception.ToString(),
                    _options.MaxRetryCount,
                    CalculateRetryDelay(message.RetryCount + 1));

                logger.LogWarning(
                    exception,
                    "Outbox mesajı yayınlanamadı. MessageId: {MessageId}, RetryCount: {RetryCount}, Poisoned: {Poisoned}",
                    message.Id,
                    message.RetryCount,
                    message.PoisonedOnUtc is not null);
            }

            // Her mesaj ayrı kalıcılaştırılır. Yayından sonra proses ölürse mesaj yeniden
            // gönderilir; bu bilinçli at-least-once semantiğidir.
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return messages.Count;
    }

    private TimeSpan CalculateRetryDelay(int retryNumber)
    {
        var exponent = Math.Max(0, retryNumber - 1);
        var seconds = _options.BaseRetryDelaySeconds * Math.Pow(2, exponent);
        return TimeSpan.FromSeconds(Math.Min(seconds, _options.MaxRetryDelaySeconds));
    }
}
