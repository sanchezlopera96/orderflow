namespace OrderFlow.Inventory.Domain;

/// <summary>Resultado de intentar reservar stock para un pedido.</summary>
public enum ReservationOutcome
{
    Reserved = 0,
    InsufficientStock = 1,
}
