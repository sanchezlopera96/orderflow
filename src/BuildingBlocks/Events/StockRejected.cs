namespace OrderFlow.BuildingBlocks.Events;

/// <summary>Published by Inventory when stock could not be reserved (e.g. insufficient stock).</summary>
public sealed record StockRejected : IntegrationEvent
{
    public required Guid OrderId { get; init; }

    public required string Sku { get; init; }

    public required string Reason { get; init; }
}
