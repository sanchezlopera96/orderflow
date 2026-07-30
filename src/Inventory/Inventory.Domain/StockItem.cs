namespace OrderFlow.Inventory.Domain;

/// <summary>
/// Raíz del agregado de inventario: el stock disponible de un SKU. La invariante es que
/// <see cref="Available"/> nunca queda negativo. La reserva es un descuento atómico; la
/// concurrencia entre mensajes que tocan el mismo SKU se resuelve con concurrencia optimista
/// mediante <see cref="Version"/>, que se usa como token de concurrencia en la infraestructura.
/// </summary>
public sealed class StockItem
{
    // Requerido por EF Core para la materialización.
    private StockItem()
    {
    }

    public StockItem(string sku, int available)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentOutOfRangeException.ThrowIfNegative(available);

        Sku = sku;
        Available = available;
    }

    public string Sku { get; private set; } = null!;

    public int Available { get; private set; }

    /// <summary>Token de concurrencia optimista. Avanza en cada reserva exitosa.</summary>
    public uint Version { get; private set; }

    /// <summary>
    /// Intenta reservar <paramref name="quantity"/> unidades. Si hay stock suficiente lo descuenta y
    /// devuelve <see cref="ReservationOutcome.Reserved"/>; si no, no modifica nada y devuelve
    /// <see cref="ReservationOutcome.InsufficientStock"/>.
    /// </summary>
    public ReservationOutcome Reserve(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        if (Available < quantity)
        {
            return ReservationOutcome.InsufficientStock;
        }

        Available -= quantity;
        Version++;
        return ReservationOutcome.Reserved;
    }
}
