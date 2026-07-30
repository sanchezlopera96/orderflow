using Microsoft.EntityFrameworkCore;
using OrderFlow.Orders.Api.Realtime;
using OrderFlow.Orders.Domain;
using OrderFlow.Orders.Infrastructure.Persistence;

namespace OrderFlow.Orders.Api.Application;

/// <summary>
/// Aplica el resultado de la reserva de stock al pedido: lo confirma o lo rechaza. La idempotencia
/// sale de la máquina de estados del dominio: si el resultado llega dos veces, la segunda es un
/// no-op porque el pedido ya no está en Pending; y una transición ilegal se registra sin romper.
/// </summary>
public sealed class StockResultHandler(
    OrdersDbContext dbContext,
    IOrderNotifier orderNotifier,
    ILogger<StockResultHandler> logger)
{
    public async Task ConfirmAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order is null)
        {
            logger.LogWarning("Pedido {OrderId} no encontrado al confirmar; se ignora", orderId);
            return;
        }

        try
        {
            order.Confirm();
        }
        catch (InvalidOrderStateException exception)
        {
            logger.LogWarning(exception, "Transición inválida al confirmar el pedido {OrderId}; se ignora", orderId);
            return;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await orderNotifier.OrderChangedAsync(OrderResponse.From(order), cancellationToken);
        logger.LogInformation("Pedido {OrderId} confirmado", orderId);
    }

    public async Task RejectAsync(Guid orderId, string reason, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order is null)
        {
            logger.LogWarning("Pedido {OrderId} no encontrado al rechazar; se ignora", orderId);
            return;
        }

        try
        {
            order.Reject();
        }
        catch (InvalidOrderStateException exception)
        {
            logger.LogWarning(exception, "Transición inválida al rechazar el pedido {OrderId}; se ignora", orderId);
            return;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await orderNotifier.OrderChangedAsync(OrderResponse.From(order), cancellationToken);
        logger.LogInformation("Pedido {OrderId} rechazado: {Reason}", orderId, reason);
    }
}
