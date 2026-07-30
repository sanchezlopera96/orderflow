using OrderFlow.Orders.Api.Application;

namespace OrderFlow.Orders.Api.Realtime;

/// <summary>Notifica a los clientes que un pedido cambió. Abstrae SignalR de la lógica de aplicación.</summary>
public interface IOrderNotifier
{
    Task OrderChangedAsync(OrderResponse order, CancellationToken cancellationToken = default);
}
