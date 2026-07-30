using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using OrderFlow.BuildingBlocks.Events;
using OrderFlow.BuildingBlocks.Messaging;
using OrderFlow.Inventory.Domain;
using OrderFlow.Inventory.Infrastructure.Persistence;

namespace OrderFlow.Inventory.Infrastructure.Messaging;

/// <summary>
/// Procesa un evento OrderCreated: reserva stock de forma idempotente y publica el resultado
/// (StockReserved o StockRejected).
///
/// Idempotencia: antes de tocar el stock se consulta el inbox (ProcessedEvents) por EventId; si ya
/// está, se ignora. El descuento de stock y el registro en el inbox se guardan en un mismo
/// SaveChanges (una transacción), así que un evento repetido nunca descuenta dos veces. Si dos
/// entregas del mismo evento corren en paralelo, la clave primaria de ProcessedEvents hace que una
/// falle con violación de unicidad, que se trata como "ya procesado".
///
/// Concurrencia: si dos eventos distintos tocan el mismo SKU a la vez, la concurrencia optimista
/// (columna Version) hace fallar a uno con <see cref="DbUpdateConcurrencyException"/>; se reintenta recargando.
/// </summary>
public sealed class OrderCreatedHandler(
    InventoryDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<OrderCreatedHandler> logger)
{
    private const int MaxConcurrencyRetries = 3;

    public async Task HandleAsync(OrderCreated message, CancellationToken cancellationToken)
    {
        var alreadyProcessed = await dbContext.ProcessedEvents
            .AsNoTracking()
            .AnyAsync(e => e.EventId == message.EventId, cancellationToken);

        if (alreadyProcessed)
        {
            logger.LogInformation("Evento {EventId} ya procesado; se ignora", message.EventId);
            return;
        }

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await ReserveAndPublishAsync(message, cancellationToken);
                return;
            }
            catch (DbUpdateConcurrencyException) when (attempt < MaxConcurrencyRetries)
            {
                logger.LogWarning(
                    "Conflicto de concurrencia al reservar {Sku} (intento {Attempt}); reintentando",
                    message.Sku,
                    attempt);
                dbContext.ChangeTracker.Clear();
            }
        }
    }

    private async Task ReserveAndPublishAsync(OrderCreated message, CancellationToken cancellationToken)
    {
        var stock = await dbContext.StockItems
            .FirstOrDefaultAsync(s => s.Sku == message.Sku, cancellationToken);

        bool reserved;
        string? rejectionReason = null;

        if (stock is null)
        {
            reserved = false;
            rejectionReason = $"El SKU '{message.Sku}' no existe en el inventario.";
        }
        else
        {
            var outcome = stock.Reserve(message.Quantity);
            reserved = outcome == ReservationOutcome.Reserved;
            if (!reserved)
            {
                rejectionReason = $"Stock insuficiente para el SKU '{message.Sku}'.";
            }
        }

        dbContext.ProcessedEvents.Add(new ProcessedEvent(message.EventId));

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateKey(exception))
        {
            // Otra entrega del mismo evento ganó la carrera: ya quedó procesado.
            logger.LogInformation("Evento {EventId} procesado de forma concurrente; se ignora el duplicado", message.EventId);
            dbContext.ChangeTracker.Clear();
            return;
        }

        if (reserved)
        {
            await eventPublisher.PublishAsync(
                new StockReserved { OrderId = message.OrderId, Sku = message.Sku, Quantity = message.Quantity },
                cancellationToken);
            logger.LogInformation(
                "Stock reservado para el pedido {OrderId} (SKU {Sku}, cantidad {Quantity})",
                message.OrderId, message.Sku, message.Quantity);
        }
        else
        {
            await eventPublisher.PublishAsync(
                new StockRejected { OrderId = message.OrderId, Sku = message.Sku, Reason = rejectionReason! },
                cancellationToken);
            logger.LogInformation(
                "Stock rechazado para el pedido {OrderId} (SKU {Sku}): {Reason}",
                message.OrderId, message.Sku, rejectionReason);
        }
    }

    private static bool IsDuplicateKey(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
