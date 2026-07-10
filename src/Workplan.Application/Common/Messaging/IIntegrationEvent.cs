namespace Workplan.Application.Common.Messaging;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
}
