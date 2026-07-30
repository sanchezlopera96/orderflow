using OrderFlow.BuildingBlocks.Events;

namespace OrderFlow.BuildingBlocks.Messaging;

/// <summary>
/// Abstraction over the message broker. Services depend on this interface, never on the
/// concrete RabbitMQ implementation, so the transport can be swapped or faked in tests.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}
