using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using OrderFlow.BuildingBlocks.Events;

namespace OrderFlow.BuildingBlocks.Messaging.RabbitMq;

/// <summary>
/// Publica eventos de integración en el topic exchange de OrderFlow. Reutiliza un canal
/// serializando las publicaciones (los canales de RabbitMQ no son seguros para uso concurrente)
/// y marca los mensajes como persistentes para que sobrevivan a un reinicio del broker.
/// </summary>
public sealed class RabbitMqEventPublisher(
    RabbitMqConnection connection,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqEventPublisher> logger) : IEventPublisher, IAsyncDisposable
{
    private readonly RabbitMqOptions _options = options.Value;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IChannel? _channel;

    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        // Se enruta por el tipo en runtime (no el genérico TEvent), para que también funcione al
        // publicar eventos reconstruidos desde el outbox como IntegrationEvent.
        var eventType = integrationEvent.GetType();
        var routingKey = IntegrationEventRouting.RoutingKeyFor(eventType);
        var body = JsonSerializer.SerializeToUtf8Bytes(integrationEvent, eventType, MessagingJson.Options);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = integrationEvent.EventId.ToString(),
            Type = eventType.Name,
        };

        await _gate.WaitAsync(cancellationToken);
        try
        {
            var channel = await EnsureChannelAsync(cancellationToken);
            await channel.BasicPublishAsync(
                exchange: _options.ExchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
        finally
        {
            _gate.Release();
        }

        logger.LogInformation(
            "Evento {EventType} publicado con routing key {RoutingKey} (EventId {EventId})",
            eventType.Name,
            routingKey,
            integrationEvent.EventId);
    }

    private async Task<IChannel> EnsureChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        var currentConnection = await connection.GetConnectionAsync(cancellationToken);
        _channel = await currentConnection.CreateChannelAsync(cancellationToken: cancellationToken);

        // Declaración idempotente del exchange: si ya existe, RabbitMQ no hace nada.
        await _channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        return _channel;
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        _gate.Dispose();
    }
}
