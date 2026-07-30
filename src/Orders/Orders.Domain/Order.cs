namespace OrderFlow.Orders.Domain;

/// <summary>
/// Raíz del agregado de pedidos. Es dueño de su ciclo de vida: nace como
/// <see cref="OrderStatus.Pending"/> y desde ahí solo puede pasar a <see cref="OrderStatus.Confirmed"/>
/// o <see cref="OrderStatus.Rejected"/>. Transicionar al estado en el que ya está es un no-op
/// idempotente, lo que permite que el consumidor de mensajería aplique con seguridad un resultado
/// de stock duplicado.
/// </summary>
public sealed class Order
{
    // Requerido por EF Core para la materialización.
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

    /// <summary>Crea un nuevo pedido en estado <see cref="OrderStatus.Pending"/>.</summary>
    public static Order Create(string customerName, string sku, int quantity) =>
        new(Guid.NewGuid(), customerName, sku, quantity, OrderStatus.Pending, DateTimeOffset.UtcNow);

    /// <summary>Confirma el pedido. Idempotente si ya está confirmado; inválido si ya fue rechazado.</summary>
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

    /// <summary>Rechaza el pedido. Idempotente si ya está rechazado; inválido si ya fue confirmado.</summary>
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
