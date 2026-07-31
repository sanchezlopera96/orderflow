using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OrderFlow.BuildingBlocks.Events;
using OrderFlow.BuildingBlocks.Messaging;
using OrderFlow.BuildingBlocks.Messaging.RabbitMq;
using OrderFlow.IntegrationTests.Fixtures;
using Xunit;

namespace OrderFlow.IntegrationTests;

/// <summary>Round-trip real por RabbitMQ: el publisher publica y un consumidor recibe el mismo evento.</summary>
[Trait("Category", "Integration")]
public sealed class RabbitMqPublisherIntegrationTests(RabbitMqFixture fixture) : IClassFixture<RabbitMqFixture>
{
    [Fact]
    public async Task A_published_order_created_is_received_from_the_topic_exchange()
    {
        var options = Options.Create(new RabbitMqOptions
        {
            HostName = fixture.HostName,
            Port = fixture.Port,
            UserName = "guest",
            Password = "guest",
            ExchangeName = "orderflow",
        });

        await using var connection = new RabbitMqConnection(options, NullLogger<RabbitMqConnection>.Instance);
        await using var publisher = new RabbitMqEventPublisher(connection, options, NullLogger<RabbitMqEventPublisher>.Instance);

        // Consumidor de prueba enlazado a la routing key de OrderCreated.
        var rawConnection = await connection.GetConnectionAsync();
        var channel = await rawConnection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync("orderflow", ExchangeType.Topic, durable: true, autoDelete: false);
        var queue = await channel.QueueDeclareAsync(queue: string.Empty, durable: false, exclusive: true, autoDelete: true);
        await channel.QueueBindAsync(queue.QueueName, "orderflow", OrderFlowTopology.OrderCreatedRoutingKey);

        var received = new TaskCompletionSource<OrderCreated>(TaskCreationOptions.RunContinuationsAsynchronously);
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, eventArgs) =>
        {
            var message = JsonSerializer.Deserialize<OrderCreated>(eventArgs.Body.Span, MessagingJson.Options);
            if (message is not null)
            {
                received.TrySetResult(message);
            }

            return Task.CompletedTask;
        };
        await channel.BasicConsumeAsync(queue.QueueName, autoAck: true, consumer);

        var sent = new OrderCreated { OrderId = Guid.NewGuid(), Sku = "ABC-01", Quantity = 2 };
        await publisher.PublishAsync(sent);

        var delivered = await received.Task.WaitAsync(TimeSpan.FromSeconds(15));

        delivered.EventId.Should().Be(sent.EventId);
        delivered.OrderId.Should().Be(sent.OrderId);
        delivered.Sku.Should().Be(sent.Sku);
        delivered.Quantity.Should().Be(sent.Quantity);

        await channel.DisposeAsync();
    }
}
