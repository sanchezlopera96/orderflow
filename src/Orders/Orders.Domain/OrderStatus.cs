namespace OrderFlow.Orders.Domain;

/// <summary>
/// Ciclo de vida de un pedido. Las únicas transiciones válidas son Pending -> Confirmed y
/// Pending -> Rejected; el modelo de dominio lo garantiza en el agregado.
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Rejected = 2,
}
