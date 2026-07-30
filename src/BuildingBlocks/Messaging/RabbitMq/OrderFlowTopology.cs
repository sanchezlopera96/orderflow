namespace OrderFlow.BuildingBlocks.Messaging.RabbitMq;

/// <summary>
/// Nombres de la topología de mensajería. Un único topic exchange durable con routing keys por
/// tipo de evento; las colas y sus bindings las declara cada consumidor (Inventory y Orders).
/// </summary>
public static class OrderFlowTopology
{
    public const string ExchangeName = "orderflow";

    public const string OrderCreatedRoutingKey = "order.created";

    public const string StockReservedRoutingKey = "stock.reserved";

    public const string StockRejectedRoutingKey = "stock.rejected";
}
