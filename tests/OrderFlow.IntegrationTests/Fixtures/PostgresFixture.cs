using Testcontainers.PostgreSql;
using Xunit;

namespace OrderFlow.IntegrationTests.Fixtures;

/// <summary>Levanta un PostgreSQL real en un contenedor para las pruebas de integración.</summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
