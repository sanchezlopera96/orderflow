namespace OrderFlow.BuildingBlocks.Events;

/// <summary>Publicado por Inventory cuando se reservó stock correctamente para un pedido.</summary>
public sealed record StockReserved : IntegrationEvent
{
    public required Guid OrderId { get; init; }

    public required string Sku { get; init; }

    public required int Quantity { get; init; }
}
