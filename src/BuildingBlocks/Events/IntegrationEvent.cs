namespace OrderFlow.BuildingBlocks.Events;

/// <summary>
/// Base type for every message exchanged between services. Each event carries its own
/// identity (<see cref="EventId"/>), which is the idempotency key consumers use to detect
/// duplicate deliveries, and the moment it occurred.
/// </summary>
public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
