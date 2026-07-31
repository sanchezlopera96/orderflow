using Testcontainers.RabbitMq;
using Xunit;

namespace OrderFlow.IntegrationTests.Fixtures;

/// <summary>Levanta un RabbitMQ real en un contenedor para las pruebas de mensajería.</summary>
public sealed class RabbitMqFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _container = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    public string HostName => _container.Hostname;

    public int Port => _container.GetMappedPublicPort(5672);

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
