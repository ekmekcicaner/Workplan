using Mediator;

namespace Workplan.Integration.Tests.TestInfrastructure;

internal sealed class NoopPublisher : IPublisher
{
    public ValueTask Publish(object notification, CancellationToken cancellationToken = default) =>
        ValueTask.CompletedTask;

    public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification =>
        ValueTask.CompletedTask;
}
