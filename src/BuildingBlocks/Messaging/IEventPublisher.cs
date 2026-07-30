using OrderFlow.BuildingBlocks.Events;

namespace OrderFlow.BuildingBlocks.Messaging;

/// <summary>
/// Abstracción sobre el message broker. Los servicios dependen de esta interfaz, nunca de la
/// implementación concreta de RabbitMQ, de modo que el transporte se puede intercambiar o
/// reemplazar por un doble en las pruebas.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}
