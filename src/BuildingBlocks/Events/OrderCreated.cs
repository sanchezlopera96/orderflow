namespace OrderFlow.BuildingBlocks.Events;

/// <summary>Published by Orders when a new order is persisted as Pending.</summary>
public sealed record OrderCreated : IntegrationEvent
{
    public required Guid OrderId { get; init; }

    public required string Sku { get; init; }

    public required int Quantity { get; init; }
}
