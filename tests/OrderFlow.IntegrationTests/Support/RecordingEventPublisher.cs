using System.Collections.Concurrent;
using OrderFlow.BuildingBlocks.Events;
using OrderFlow.BuildingBlocks.Messaging;

namespace OrderFlow.IntegrationTests.Support;

/// <summary>Publisher de prueba, seguro para uso concurrente, que registra lo publicado.</summary>
public sealed class RecordingEventPublisher : IEventPublisher
{
    private readonly ConcurrentBag<IntegrationEvent> _published = [];

    public IReadOnlyCollection<IntegrationEvent> Published => _published;

    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        _published.Add(integrationEvent);
        return Task.CompletedTask;
    }
}
