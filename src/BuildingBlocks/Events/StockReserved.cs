namespace OrderFlow.BuildingBlocks.Events;

/// <summary>Published by Inventory when stock was successfully reserved for an order.</summary>
public sealed record StockReserved : IntegrationEvent
{
    public required Guid OrderId { get; init; }

    public required string Sku { get; init; }

    public required int Quantity { get; init; }
}
