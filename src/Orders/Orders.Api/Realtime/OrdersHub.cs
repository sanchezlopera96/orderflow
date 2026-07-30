using Microsoft.AspNetCore.SignalR;

namespace OrderFlow.Orders.Api.Realtime;

/// <summary>
/// Hub de tiempo real. El servidor empuja el evento "OrderChanged" a los clientes conectados cada
/// vez que un pedido se crea o cambia de estado; los clientes solo escuchan.
/// </summary>
public sealed class OrdersHub : Hub;
