using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using OrderFlow.BuildingBlocks.Events;
using OrderFlow.IntegrationTests.Fixtures;
using OrderFlow.IntegrationTests.Support;
using OrderFlow.Inventory.Infrastructure.Messaging;
using OrderFlow.Inventory.Infrastructure.Persistence;
using Xunit;

namespace OrderFlow.IntegrationTests;

/// <summary>
/// Pruebas del consumidor contra un PostgreSQL real. Aquí sí se ejercitan la restricción de unicidad
/// del inbox y la concurrencia optimista (columna de versión), cosas que el proveedor en memoria no
/// puede reproducir. Cada prueba usa su propia base recién creada, para quedar aislada.
/// </summary>
[Trait("Category", "Integration")]
public sealed class InventoryHandlerIntegrationTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    /// <summary>Crea una base nueva y única, le aplica el esquema y el seed, y devuelve su connection string.</summary>
    private async Task<string> CreateFreshDatabaseAsync()
    {
        var databaseName = $"inv_{Guid.NewGuid():N}";

        await using (var admin = new NpgsqlConnection(fixture.ConnectionString))
        {
            await admin.OpenAsync();
            await using var command = admin.CreateCommand();
            command.CommandText = $"CREATE DATABASE \"{databaseName}\"";
            await command.ExecuteNonQueryAsync();
        }

        var connectionString = new NpgsqlConnectionStringBuilder(fixture.ConnectionString)
        {
            Database = databaseName,
        }.ConnectionString;

        await using (var db = ContextFor(connectionString))
        {
            await db.Database.EnsureCreatedAsync(); // crea el esquema y aplica el seed de stock
        }

        return connectionString;
    }

    private static InventoryDbContext ContextFor(string connectionString) =>
        new(new DbContextOptionsBuilder<InventoryDbContext>().UseNpgsql(connectionString).Options);

    private static OrderCreatedHandler HandlerFor(InventoryDbContext db, RecordingEventPublisher publisher) =>
        new(db, publisher, NullLogger<OrderCreatedHandler>.Instance);

    private static async Task<int> AvailableAsync(InventoryDbContext db, string sku) =>
        (await db.StockItems.AsNoTracking().SingleAsync(s => s.Sku == sku)).Available;

    [Fact]
    public async Task Reserving_stock_persists_the_decrement_and_the_inbox_entry()
    {
        var connectionString = await CreateFreshDatabaseAsync();
        var publisher = new RecordingEventPublisher();
        var message = new OrderCreated { OrderId = Guid.NewGuid(), Sku = "ABC-01", Quantity = 4 };

        await using (var db = ContextFor(connectionString))
        {
            await HandlerFor(db, publisher).HandleAsync(message, CancellationToken.None);
        }

        await using var verify = ContextFor(connectionString);
        (await AvailableAsync(verify, "ABC-01")).Should().Be(96);
        (await verify.ProcessedEvents.AnyAsync(e => e.EventId == message.EventId)).Should().BeTrue();
        publisher.Published.Should().ContainSingle().Which.Should().BeOfType<StockReserved>();
    }

    [Fact]
    public async Task The_same_event_delivered_twice_decrements_stock_only_once()
    {
        var connectionString = await CreateFreshDatabaseAsync();
        var publisher = new RecordingEventPublisher();
        var message = new OrderCreated { OrderId = Guid.NewGuid(), Sku = "ABC-01", Quantity = 5 };

        await using (var first = ContextFor(connectionString))
        {
            await HandlerFor(first, publisher).HandleAsync(message, CancellationToken.None);
        }
        await using (var second = ContextFor(connectionString))
        {
            await HandlerFor(second, publisher).HandleAsync(message, CancellationToken.None);
        }

        await using var verify = ContextFor(connectionString);
        (await AvailableAsync(verify, "ABC-01")).Should().Be(95);
        publisher.Published.Should().ContainSingle();
    }

    [Fact]
    public async Task Concurrent_duplicate_deliveries_decrement_stock_only_once()
    {
        var connectionString = await CreateFreshDatabaseAsync();
        var publisher = new RecordingEventPublisher();
        var message = new OrderCreated { OrderId = Guid.NewGuid(), Sku = "ABC-01", Quantity = 5 };

        // Dos entregas del MISMO evento en paralelo (simula dos instancias del worker).
        await using var db1 = ContextFor(connectionString);
        await using var db2 = ContextFor(connectionString);
        await Task.WhenAll(
            HandlerFor(db1, publisher).HandleAsync(message, CancellationToken.None),
            HandlerFor(db2, publisher).HandleAsync(message, CancellationToken.None));

        await using var verify = ContextFor(connectionString);
        (await AvailableAsync(verify, "ABC-01")).Should().Be(95); // la PK del inbox impide el doble descuento
        publisher.Published.Should().ContainSingle();
    }

    [Fact]
    public async Task Concurrent_reservations_on_the_same_sku_do_not_lose_updates()
    {
        var connectionString = await CreateFreshDatabaseAsync();
        var publisher = new RecordingEventPublisher();
        var first = new OrderCreated { OrderId = Guid.NewGuid(), Sku = "ABC-01", Quantity = 10 };
        var second = new OrderCreated { OrderId = Guid.NewGuid(), Sku = "ABC-01", Quantity = 10 };

        // Dos eventos DISTINTOS sobre el mismo SKU en paralelo: la concurrencia optimista hace que
        // uno reintente, y ambos descuentos terminan aplicándose (sin lost update).
        await using var db1 = ContextFor(connectionString);
        await using var db2 = ContextFor(connectionString);
        await Task.WhenAll(
            HandlerFor(db1, publisher).HandleAsync(first, CancellationToken.None),
            HandlerFor(db2, publisher).HandleAsync(second, CancellationToken.None));

        await using var verify = ContextFor(connectionString);
        (await AvailableAsync(verify, "ABC-01")).Should().Be(80);
        publisher.Published.Should().HaveCount(2);
    }
}
