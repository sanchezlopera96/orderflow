using Microsoft.AspNetCore.SignalR;
using OrderFlow.Orders.Api.Application;

namespace OrderFlow.Orders.Api.Realtime;

/// <summary>Implementación de <see cref="IOrderNotifier"/> que emite por SignalR a todos los clientes.</summary>
public sealed class SignalROrderNotifier(IHubContext<OrdersHub> hubContext) : IOrderNotifier
{
    public Task OrderChangedAsync(OrderResponse order, CancellationToken cancellationToken = default) =>
        hubContext.Clients.All.SendAsync("OrderChanged", order, cancellationToken);
}
