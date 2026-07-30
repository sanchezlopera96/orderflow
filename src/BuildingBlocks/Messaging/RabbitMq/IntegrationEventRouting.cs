using OrderFlow.BuildingBlocks.Events;

namespace OrderFlow.BuildingBlocks.Messaging.RabbitMq;

/// <summary>Resuelve la routing key que le corresponde a cada tipo de evento de integración.</summary>
public static class IntegrationEventRouting
{
    private static readonly IReadOnlyDictionary<Type, string> RoutingKeys = new Dictionary<Type, string>
    {
        [typeof(OrderCreated)] = OrderFlowTopology.OrderCreatedRoutingKey,
        [typeof(StockReserved)] = OrderFlowTopology.StockReservedRoutingKey,
        [typeof(StockRejected)] = OrderFlowTopology.StockRejectedRoutingKey,
    };

    public static string RoutingKeyFor(Type eventType) =>
        RoutingKeys.TryGetValue(eventType, out var routingKey)
            ? routingKey
            : throw new InvalidOperationException($"No hay routing key registrada para el evento {eventType.Name}.");
}
