namespace OrderFlow.BuildingBlocks.Events;

/// <summary>
/// Tipo base de todo mensaje intercambiado entre servicios. Cada evento lleva su propia
/// identidad (<see cref="EventId"/>), que es la clave de idempotencia con la que los consumidores
/// detectan entregas duplicadas, y el momento en que ocurrió.
/// </summary>
public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
