using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Workplan.Application.Common.Messaging;
using Workplan.Infrastructure.Persistence;

namespace Workplan.Infrastructure.Messaging.Outbox;

public sealed class OutboxProcessor
{
    private readonly AppDbContext _dbContext;
    private readonly IIntegrationEventPublisher _publisher;
    private readonly OutboxOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        AppDbContext dbContext,
        IIntegrationEventPublisher publisher,
        IOptions<OutboxOptions> options,
        TimeProvider timeProvider,
        ILogger<OutboxProcessor> logger)
    {
        _dbContext = dbContext;
        _publisher = publisher;
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var messages = await _dbContext.OutboxMessages
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
                await _publisher.PublishAsync(integrationEvent, cancellationToken);
                message.MarkProcessed(_timeProvider.GetUtcNow().UtcDateTime);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                var failedOnUtc = _timeProvider.GetUtcNow().UtcDateTime;
                message.RecordFailure(
                    failedOnUtc,
                    exception.ToString(),
                    _options.MaxRetryCount,
                    CalculateRetryDelay(message.RetryCount + 1));

                _logger.LogWarning(
                    exception,
                    "Outbox mesajı yayınlanamadı. MessageId: {MessageId}, RetryCount: {RetryCount}, Poisoned: {Poisoned}",
                    message.Id,
                    message.RetryCount,
                    message.PoisonedOnUtc is not null);
            }

            // Her mesaj ayrı kalıcılaştırılır. Yayından sonra proses ölürse mesaj yeniden
            // gönderilir; bu bilinçli at-least-once semantiğidir.
            await _dbContext.SaveChangesAsync(cancellationToken);
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
