using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderFlow.BuildingBlocks.Messaging.RabbitMq;

/// <summary>
/// Base para los consumidores de RabbitMQ. Centraliza el andamiaje delicado: declara el exchange y
/// una cola durable, la enlaza a sus routing keys, procesa un mensaje a la vez y hace ack al éxito,
/// nack con requeue ante un fallo transitorio y nack sin requeue ante un mensaje que no se puede
/// deserializar (para no bloquear la cola). Si el broker no está disponible al arrancar o se pierde
/// la conexión, reintenta cada pocos segundos. Cada consumidor concreto solo aporta el nombre de la
/// cola, sus routing keys y cómo despachar el cuerpo del mensaje a su handler.
/// </summary>
public abstract class RabbitMqConsumer(
    RabbitMqConnection connection,
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    private readonly RabbitMqOptions _options = options.Value;
    private IChannel? _channel;
    private CancellationToken _stoppingToken;

    protected abstract string QueueName { get; }

    protected abstract IReadOnlyCollection<string> RoutingKeys { get; }

    /// <summary>
    /// Deserializa el cuerpo según la routing key y lo entrega al handler correspondiente, resuelto
    /// del <paramref name="services"/> del scope creado para este mensaje. Debe lanzar
    /// <see cref="JsonException"/> si el mensaje no se puede interpretar (se tratará como veneno).
    /// </summary>
    protected abstract Task DispatchAsync(
        IServiceProvider services,
        string routingKey,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await StartConsumingAsync(stoppingToken);
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "El consumidor de {Queue} no pudo iniciar o perdió la conexión; reintenta en {Delay}s",
                    QueueName,
                    RetryDelay.TotalSeconds);

                await DisposeChannelAsync();

                try
                {
                    await Task.Delay(RetryDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        await DisposeChannelAsync();
    }

    private async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        var currentConnection = await connection.GetConnectionAsync(cancellationToken);
        _channel = await currentConnection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        foreach (var routingKey in RoutingKeys)
        {
            await _channel.QueueBindAsync(
                queue: QueueName,
                exchange: _options.ExchangeName,
                routingKey: routingKey,
                arguments: null,
                cancellationToken: cancellationToken);
        }

        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageAsync;

        await _channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        logger.LogInformation("Consumidor de {Queue} iniciado", QueueName);
    }

    private async Task OnMessageAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            await DispatchAsync(scope.ServiceProvider, eventArgs.RoutingKey, eventArgs.Body, _stoppingToken);
            await _channel!.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, _stoppingToken);
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "Mensaje no interpretable en {Queue}; se descarta", QueueName);
            await _channel!.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, _stoppingToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error al procesar un mensaje en {Queue}; se reencola", QueueName);
            await _channel!.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true, _stoppingToken);
        }
    }

    private async Task DisposeChannelAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
            _channel = null;
        }
    }
}
