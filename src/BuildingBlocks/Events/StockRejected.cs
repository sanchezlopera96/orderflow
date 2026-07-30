namespace OrderFlow.BuildingBlocks.Events;

/// <summary>Publicado por Inventory cuando no se pudo reservar stock (por ejemplo, stock insuficiente).</summary>
public sealed record StockRejected : IntegrationEvent
{
    public required Guid OrderId { get; init; }

    public required string Sku { get; init; }

    public required string Reason { get; init; }
}
