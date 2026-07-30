namespace OrderFlow.Orders.Domain;

/// <summary>
/// Se lanza cuando se intenta una transición de estado que el agregado prohíbe (por ejemplo,
/// confirmar un pedido que ya fue rechazado). Las transiciones al mismo estado se tratan como
/// no-ops idempotentes y no lanzan esta excepción.
/// </summary>
public sealed class InvalidOrderStateException(OrderStatus from, OrderStatus to)
    : Exception($"No se puede pasar un pedido de {from} a {to}.")
{
    public OrderStatus From { get; } = from;

    public OrderStatus To { get; } = to;
}
