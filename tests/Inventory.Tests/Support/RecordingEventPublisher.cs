using OrderFlow.BuildingBlocks.Events;
using OrderFlow.BuildingBlocks.Messaging;

namespace OrderFlow.Inventory.Tests.Support;

/// <summary>Doble de prueba que registra los eventos publicados en lugar de enviarlos a un broker.</summary>
public sealed class RecordingEventPublisher : IEventPublisher
{
    public List<IntegrationEvent> Published { get; } = [];

    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        Published.Add(integrationEvent);
        return Task.CompletedTask;
    }
}
