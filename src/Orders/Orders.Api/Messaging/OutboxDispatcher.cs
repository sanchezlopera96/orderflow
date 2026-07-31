using OrderFlow.Orders.Infrastructure.Messaging;

namespace OrderFlow.Orders.Api.Messaging;

/// <summary>Publica periódicamente los mensajes pendientes del outbox mediante el <see cref="OutboxProcessor"/>.</summary>
public sealed class OutboxDispatcher(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxDispatcher> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
                    await processor.ProcessPendingAsync(stoppingToken);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Error en el despachador del outbox; se reintentará en el siguiente ciclo");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Apagado normal.
        }
    }
}
