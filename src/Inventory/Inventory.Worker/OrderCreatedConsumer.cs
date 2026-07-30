using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderFlow.BuildingBlocks.Events;
using OrderFlow.BuildingBlocks.Messaging;
using OrderFlow.BuildingBlocks.Messaging.RabbitMq;
using OrderFlow.Inventory.Infrastructure.Messaging;

namespace OrderFlow.Inventory.Worker;

/// <summary>Consume OrderCreated y delega la reserva de stock en el <see cref="OrderCreatedHandler"/>.</summary>
public sealed class OrderCreatedConsumer(
    RabbitMqConnection connection,
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<OrderCreatedConsumer> logger)
    : RabbitMqConsumer(connection, options, scopeFactory, logger)
{
    protected override string QueueName => "inventory.order-created";

    protected override IReadOnlyCollection<string> RoutingKeys => [OrderFlowTopology.OrderCreatedRoutingKey];

    protected override async Task DispatchAsync(
        IServiceProvider services,
        string routingKey,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Deserialize<OrderCreated>(body.Span, MessagingJson.Options)
            ?? throw new JsonException("El mensaje OrderCreated está vacío.");

        var handler = services.GetRequiredService<OrderCreatedHandler>();
        await handler.HandleAsync(message, cancellationToken);
    }
}
