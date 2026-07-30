using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OrderFlow.BuildingBlocks.Messaging.RabbitMq;

/// <summary>Health check que verifica que la conexión con RabbitMQ esté abierta.</summary>
public sealed class RabbitMqHealthCheck(RabbitMqConnection connection) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentConnection = await connection.GetConnectionAsync(cancellationToken);
            return currentConnection.IsOpen
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("La conexión con RabbitMQ no está abierta.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("No se pudo conectar con RabbitMQ.", exception);
        }
    }
}
