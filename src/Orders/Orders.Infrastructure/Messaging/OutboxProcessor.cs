using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.BuildingBlocks.Messaging;
using OrderFlow.Orders.Infrastructure.Persistence;

namespace OrderFlow.Orders.Infrastructure.Messaging;

/// <summary>
/// Publica los mensajes pendientes del outbox y los marca como procesados. Si la publicación falla,
/// registra el error e incrementa los intentos, dejando el mensaje para el siguiente ciclo.
/// </summary>
public sealed class OutboxProcessor(
    OrdersDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<OutboxProcessor> logger)
{
    private const int BatchSize = 20;

    public async Task<int> ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        var pending = await dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.OccurredAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        var processed = 0;

        foreach (var message in pending)
        {
            try
            {
                var integrationEvent = OutboxSerializer.Deserialize(message);
                await eventPublisher.PublishAsync(integrationEvent, cancellationToken);
                message.MarkProcessed();
                processed++;
            }
            catch (Exception exception)
            {
                message.RecordFailure(exception.Message);
                logger.LogWarning(exception, "No se pudo publicar el mensaje de outbox {OutboxId}; se reintentará", message.Id);
            }
        }

        if (pending.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return processed;
    }
}
