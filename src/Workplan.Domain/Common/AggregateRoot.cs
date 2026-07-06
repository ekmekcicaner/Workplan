using Mediator;

namespace Workplan.Domain.Common;

public abstract class AggregateRoot<TId>
{
    public TId Id { get; protected set; } = default!;
    
    private readonly List<INotification> _domainEvents = new();
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    protected void Raise(INotification domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}