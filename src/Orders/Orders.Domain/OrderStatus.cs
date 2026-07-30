namespace OrderFlow.Orders.Domain;

/// <summary>
/// Lifecycle of an order. The only valid transitions are Pending -> Confirmed and
/// Pending -> Rejected; the domain model enforces this in later stages.
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Rejected = 2,
}
