namespace OrderFlow.Orders.Domain;

/// <summary>
/// The order aggregate root. It owns its lifecycle: it is created as <see cref="OrderStatus.Pending"/>
/// and can only move to <see cref="OrderStatus.Confirmed"/> or <see cref="OrderStatus.Rejected"/> from
/// there. Transitioning to the state it is already in is an idempotent no-op, which lets the messaging
/// consumer apply a duplicated stock result safely.
/// </summary>
public sealed class Order
{
    // Required by EF Core for materialization.
    private Order()
    {
    }

    private Order(Guid id, string customerName, string sku, int quantity, OrderStatus status, DateTimeOffset createdAt)
    {
        Id = id;
        CustomerName = customerName;
        Sku = sku;
        Quantity = quantity;
        Status = status;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public string CustomerName { get; private set; } = null!;

    public string Sku { get; private set; } = null!;

    public int Quantity { get; private set; }

    public OrderStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Creates a new order in the <see cref="OrderStatus.Pending"/> state.</summary>
    public static Order Create(string customerName, string sku, int quantity) =>
        new(Guid.NewGuid(), customerName, sku, quantity, OrderStatus.Pending, DateTimeOffset.UtcNow);

    /// <summary>Confirms the order. Idempotent if already confirmed; invalid if already rejected.</summary>
    public void Confirm()
    {
        if (Status == OrderStatus.Confirmed)
        {
            return;
        }

        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOrderStateException(Status, OrderStatus.Confirmed);
        }

        Status = OrderStatus.Confirmed;
    }

    /// <summary>Rejects the order. Idempotent if already rejected; invalid if already confirmed.</summary>
    public void Reject()
    {
        if (Status == OrderStatus.Rejected)
        {
            return;
        }

        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOrderStateException(Status, OrderStatus.Rejected);
        }

        Status = OrderStatus.Rejected;
    }
}
