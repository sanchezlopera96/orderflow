namespace OrderFlow.BuildingBlocks.Events;

/// <summary>Publicado por Orders cuando se persiste un nuevo pedido en estado Pending.</summary>
public sealed record OrderCreated : IntegrationEvent
{
    public required Guid OrderId { get; init; }

    public required string Sku { get; init; }

    public required int Quantity { get; init; }
}
