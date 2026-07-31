using OrderFlow.Orders.Api.Application;
using OrderFlow.Orders.Api.Realtime;

namespace OrderFlow.IntegrationTests.Support;

/// <summary>Notificador de tiempo real que no hace nada (para las pruebas del OrderService).</summary>
public sealed class NoOpOrderNotifier : IOrderNotifier
{
    public Task OrderChangedAsync(OrderResponse order, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
