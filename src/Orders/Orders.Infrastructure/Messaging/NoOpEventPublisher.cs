using Microsoft.Extensions.Logging;
using OrderFlow.BuildingBlocks.Events;
using OrderFlow.BuildingBlocks.Messaging;

namespace OrderFlow.Orders.Infrastructure.Messaging;

/// <summary>
/// Temporary <see cref="IEventPublisher"/> that only logs. It keeps the create-order flow complete
/// and testable before the real RabbitMQ publisher is introduced in the messaging stage.
/// </summary>
public sealed class NoOpEventPublisher(ILogger<NoOpEventPublisher> logger) : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        logger.LogInformation(
            "[NoOp] Would publish {EventType} with EventId {EventId}",
            typeof(TEvent).Name,
            integrationEvent.EventId);

        return Task.CompletedTask;
    }
}
