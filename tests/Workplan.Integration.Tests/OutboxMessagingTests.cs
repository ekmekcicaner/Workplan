using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Workplan.Application.Common.Messaging;
using Workplan.Application.Features.DailyPlans.IntegrationEvents;
using Workplan.Domain.Entities;
using Workplan.Domain.Enums;
using Workplan.Domain.Events;
using Workplan.Domain.ValueObjects;
using Workplan.Infrastructure.Messaging.Outbox;
using Workplan.Infrastructure.Persistence;
using Xunit;

namespace Workplan.Integration.Tests;

public class OutboxMessagingTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Final_approval_and_save_persist_one_stable_outbox_message_and_clear_domain_events()
    {
        await using var db = CreateDbContext();
        var plan = CreateFullyApprovedPlan();
        var domainEvent = plan.DomainEvents
            .OfType<DailyWorkApprovedFullyEvent>()
            .Should().ContainSingle().Subject;
        var expectedPayload = JsonSerializer.Serialize(
            new DailyPlanFullyApproved(domainEvent.EventId, domainEvent.OccurredOnUtc, plan.Id),
            JsonOptions);

        db.DailyPlans.Add(plan);
        await db.SaveChangesAsync(CancellationToken.None);
        await db.SaveChangesAsync(CancellationToken.None);

        var outboxMessage = await db.OutboxMessages.SingleAsync();
        outboxMessage.Id.Should().Be(domainEvent.EventId);
        outboxMessage.Type.Should().Be(DailyPlanFullyApproved.EventName);
        outboxMessage.Payload.Should().Be(expectedPayload);
        outboxMessage.OccurredOnUtc.Should().Be(domainEvent.OccurredOnUtc);
        outboxMessage.ProcessedOnUtc.Should().BeNull();
        plan.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ProcessBatch_publishes_due_message_and_marks_it_processed()
    {
        await using var db = CreateDbContext();
        var plan = CreateFullyApprovedPlan();
        db.DailyPlans.Add(plan);
        await db.SaveChangesAsync(CancellationToken.None);
        var pendingMessage = await db.OutboxMessages.SingleAsync();

        var publisher = new RecordingPublisher();
        var timeProvider = new MutableTimeProvider(DateTimeOffset.UtcNow.AddMinutes(1));
        var processor = CreateProcessor(db, publisher, timeProvider);

        var processedCount = await processor.ProcessBatchAsync(CancellationToken.None);

        processedCount.Should().Be(1);
        var publishedEvent = publisher.PublishedEvents
            .Should().ContainSingle().Subject
            .Should().BeOfType<DailyPlanFullyApproved>().Subject;
        publishedEvent.EventId.Should().Be(pendingMessage.Id);
        publishedEvent.OccurredOnUtc.Should().Be(pendingMessage.OccurredOnUtc);
        publishedEvent.DailyPlanId.Should().Be(plan.Id);

        var outboxMessage = await db.OutboxMessages.SingleAsync();
        outboxMessage.ProcessedOnUtc.Should().Be(timeProvider.GetUtcNow().UtcDateTime);
        outboxMessage.RetryCount.Should().Be(0);
        outboxMessage.PoisonedOnUtc.Should().BeNull();
        outboxMessage.LastError.Should().BeNull();

        (await processor.ProcessBatchAsync(CancellationToken.None)).Should().Be(0);
        publisher.AttemptCount.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ProcessBatch_publisher_failures_increment_retry_and_poison_at_retry_limit()
    {
        await using var db = CreateDbContext();
        var plan = CreateFullyApprovedPlan();
        db.DailyPlans.Add(plan);
        await db.SaveChangesAsync(CancellationToken.None);

        var publisher = new RecordingPublisher(new InvalidOperationException("webhook unavailable"));
        var timeProvider = new MutableTimeProvider(DateTimeOffset.UtcNow.AddMinutes(1));
        var processor = CreateProcessor(db, publisher, timeProvider, maxRetryCount: 3);

        (await processor.ProcessBatchAsync(CancellationToken.None)).Should().Be(1);
        var outboxMessage = await db.OutboxMessages.SingleAsync();
        outboxMessage.RetryCount.Should().Be(1);
        outboxMessage.PoisonedOnUtc.Should().BeNull();
        outboxMessage.NextAttemptOnUtc.Should().Be(timeProvider.GetUtcNow().UtcDateTime.AddSeconds(2));

        timeProvider.Advance(TimeSpan.FromSeconds(2));
        (await processor.ProcessBatchAsync(CancellationToken.None)).Should().Be(1);
        outboxMessage.RetryCount.Should().Be(2);
        outboxMessage.PoisonedOnUtc.Should().BeNull();
        outboxMessage.NextAttemptOnUtc.Should().Be(timeProvider.GetUtcNow().UtcDateTime.AddSeconds(4));

        timeProvider.Advance(TimeSpan.FromSeconds(4));
        (await processor.ProcessBatchAsync(CancellationToken.None)).Should().Be(1);

        outboxMessage.RetryCount.Should().Be(3);
        outboxMessage.PoisonedOnUtc.Should().Be(timeProvider.GetUtcNow().UtcDateTime);
        outboxMessage.ProcessedOnUtc.Should().BeNull();
        outboxMessage.LastError.Should().Contain("webhook unavailable");
        publisher.AttemptCount.Should().Be(3);

        (await processor.ProcessBatchAsync(CancellationToken.None)).Should().Be(0);
        publisher.AttemptCount.Should().Be(3);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"outbox-tests-{Guid.NewGuid():N}")
            .AddInterceptors(new OutboxSaveChangesInterceptor())
            .Options;

        return new AppDbContext(options);
    }

    private static OutboxProcessor CreateProcessor(
        AppDbContext db,
        IIntegrationEventPublisher publisher,
        TimeProvider timeProvider,
        int maxRetryCount = 3) =>
        new(
            db,
            publisher,
            Options.Create(new OutboxOptions
            {
                BatchSize = 10,
                MaxRetryCount = maxRetryCount,
                BaseRetryDelaySeconds = 2,
                MaxRetryDelaySeconds = 30
            }),
            timeProvider,
            NullLogger<OutboxProcessor>.Instance);

    private static DailyPlan CreateFullyApprovedPlan()
    {
        var headOfMasterId = Guid.NewGuid();
        var plan = DailyPlan.CreateFromPlan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            Guid.NewGuid(),
            headOfMasterId,
            10,
            2,
            Unit.M2).Value;

        plan.StartWork(headOfMasterId, Guid.NewGuid()).IsSuccess.Should().BeTrue();
        plan.SubmitProgress(10, 2, 0, "completed", headOfMasterId)
            .IsSuccess.Should().BeTrue();
        plan.Approve(WorkStatus.ApprovedBySiteChief, Guid.NewGuid(), Guid.NewGuid())
            .IsSuccess.Should().BeTrue();
        plan.Approve(WorkStatus.ApprovedByPM, Guid.NewGuid(), Guid.NewGuid())
            .IsSuccess.Should().BeTrue();

        return plan;
    }

    private sealed class RecordingPublisher(Exception? exception = null) : IIntegrationEventPublisher
    {
        public List<IIntegrationEvent> PublishedEvents { get; } = new();
        public int AttemptCount { get; private set; }

        public Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken)
        {
            AttemptCount++;

            if (exception is not null)
                throw exception;

            PublishedEvents.Add(integrationEvent);
            return Task.CompletedTask;
        }
    }

    private sealed class MutableTimeProvider(DateTimeOffset initialUtcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = initialUtcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan duration) => _utcNow = _utcNow.Add(duration);
    }
}
