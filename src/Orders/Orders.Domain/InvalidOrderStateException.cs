namespace OrderFlow.Orders.Domain;

/// <summary>
/// Thrown when a caller attempts an order state transition that the aggregate forbids
/// (for example, confirming an order that was already rejected). Same-state transitions are
/// treated as idempotent no-ops and do not raise this exception.
/// </summary>
public sealed class InvalidOrderStateException(OrderStatus from, OrderStatus to)
    : Exception($"Cannot transition an order from {from} to {to}.")
{
    public OrderStatus From { get; } = from;

    public OrderStatus To { get; } = to;
}
