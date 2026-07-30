using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OrderFlow.BuildingBlocks.Events;
using OrderFlow.BuildingBlocks.Messaging;
using OrderFlow.BuildingBlocks.Messaging.RabbitMq;
using OrderFlow.Inventory.Infrastructure.Messaging;

namespace OrderFlow.Inventory.Worker;

/// <summary>
/// Consume el evento OrderCreated desde su cola durable y delega el procesamiento en el
/// <see cref="OrderCreatedHandler"/>. Confirma (ack) el mensaje cuando se procesa bien; ante un
/// fallo transitorio lo devuelve a la cola (nack con requeue), y ante un mensaje que no se puede
/// deserializar lo descarta (nack sin requeue) para no bloquear la cola.
/// </summary>
public sealed class OrderCreatedConsumer(
    RabbitMqConnection connection,
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<OrderCreatedConsumer> logger) : BackgroundService
{
    private const string QueueName = "inventory.order-created";

    private readonly RabbitMqOptions _options = options.Value;
    private IChannel? _channel;
    private CancellationToken _stoppingToken;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;

        try
        {
            var currentConnection = await connection.GetConnectionAsync(stoppingToken);
            _channel = await currentConnection.CreateChannelAsync(cancellationToken: stoppingToken);

            await _channel.ExchangeDeclareAsync(
                exchange: _options.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            await _channel.QueueBindAsync(
                queue: QueueName,
                exchange: _options.ExchangeName,
                routingKey: OrderFlowTopology.OrderCreatedRoutingKey,
                arguments: null,
                cancellationToken: stoppingToken);

            // Un mensaje a la vez: facilita el orden y hace demostrable la idempotencia.
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += OnMessageAsync;

            await _channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            logger.LogInformation("Consumidor de {Queue} iniciado", QueueName);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Apagado normal.
        }
        finally
        {
            if (_channel is not null)
            {
                await _channel.DisposeAsync();
            }
        }
    }

    private async Task OnMessageAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        OrderCreated? message;
        try
        {
            message = JsonSerializer.Deserialize<OrderCreated>(eventArgs.Body.Span, MessagingJson.Options);
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "No se pudo deserializar el mensaje; se descarta");
            await _channel!.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, _stoppingToken);
            return;
        }

        if (message is null)
        {
            logger.LogError("Mensaje vacío; se descarta");
            await _channel!.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, _stoppingToken);
            return;
        }

        try
        {
            using var scope = scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<OrderCreatedHandler>();
            await handler.HandleAsync(message, _stoppingToken);

            await _channel!.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, _stoppingToken);
        }
        catch (Exception exception)
        {
            // Fallo transitorio (por ejemplo, la base de datos no responde): se devuelve a la cola
            // para reintentarlo. Como el consumidor es idempotente, reprocesar es seguro.
            logger.LogError(exception, "Error al procesar OrderCreated del pedido {OrderId}; se reencola", message.OrderId);
            await _channel!.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true, _stoppingToken);
        }
    }
}
