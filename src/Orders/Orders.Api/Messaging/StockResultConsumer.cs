using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderFlow.BuildingBlocks.Events;
using OrderFlow.BuildingBlocks.Messaging;
using OrderFlow.BuildingBlocks.Messaging.RabbitMq;
using OrderFlow.Orders.Api.Application;

namespace OrderFlow.Orders.Api.Messaging;

/// <summary>
/// Consume los resultados de stock (StockReserved / StockRejected) y aplica la transición al pedido
/// mediante el <see cref="StockResultHandler"/>.
/// </summary>
public sealed class StockResultConsumer(
    RabbitMqConnection connection,
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<StockResultConsumer> logger)
    : RabbitMqConsumer(connection, options, scopeFactory, logger)
{
    protected override string QueueName => "orders.stock-results";

    protected override IReadOnlyCollection<string> RoutingKeys =>
        [OrderFlowTopology.StockReservedRoutingKey, OrderFlowTopology.StockRejectedRoutingKey];

    protected override async Task DispatchAsync(
        IServiceProvider services,
        string routingKey,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken)
    {
        var handler = services.GetRequiredService<StockResultHandler>();

        if (routingKey == OrderFlowTopology.StockReservedRoutingKey)
        {
            var reserved = JsonSerializer.Deserialize<StockReserved>(body.Span, MessagingJson.Options)
                ?? throw new JsonException("El mensaje StockReserved está vacío.");
            await handler.ConfirmAsync(reserved.OrderId, cancellationToken);
        }
        else if (routingKey == OrderFlowTopology.StockRejectedRoutingKey)
        {
            var rejected = JsonSerializer.Deserialize<StockRejected>(body.Span, MessagingJson.Options)
                ?? throw new JsonException("El mensaje StockRejected está vacío.");
            await handler.RejectAsync(rejected.OrderId, rejected.Reason, cancellationToken);
        }
    }
}
