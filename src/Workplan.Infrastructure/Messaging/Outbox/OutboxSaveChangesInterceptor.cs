using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Workplan.Domain.Common;
using Workplan.Infrastructure.Persistence;

namespace Workplan.Infrastructure.Messaging.Outbox;

public sealed class OutboxSaveChangesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AddOutboxMessages(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddOutboxMessages(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        ClearDomainEvents(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        ClearDomainEvents(eventData.Context);
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private static void AddOutboxMessages(DbContext? context)
    {
        if (context is not AppDbContext dbContext)
            return;

        var domainEvents = dbContext.ChangeTracker
            .Entries<AggregateRoot<Guid>>()
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            if (dbContext.OutboxMessages.Local.Any(message => message.Id == domainEvent.EventId))
                continue;

            dbContext.OutboxMessages.Add(
                IntegrationEventSerializer.CreateOutboxMessage(domainEvent));
        }
    }

    private static void ClearDomainEvents(DbContext? context)
    {
        if (context is not AppDbContext dbContext)
            return;

        foreach (var aggregate in dbContext.ChangeTracker
                     .Entries<AggregateRoot<Guid>>()
                     .Select(entry => entry.Entity))
        {
            aggregate.ClearDomainEvents();
        }
    }
}
