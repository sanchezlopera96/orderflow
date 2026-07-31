using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OrderFlow.BuildingBlocks.Events;
using OrderFlow.IntegrationTests.Fixtures;
using OrderFlow.IntegrationTests.Support;
using OrderFlow.Orders.Api.Application;
using OrderFlow.Orders.Infrastructure.Messaging;
using OrderFlow.Orders.Infrastructure.Persistence;
using Xunit;

namespace OrderFlow.IntegrationTests;

/// <summary>
/// Pruebas del outbox transaccional contra PostgreSQL real: crear un pedido guarda el evento en el
/// outbox en la misma transacción (sin tocar el broker), y el despachador lo publica después.
/// </summary>
[Trait("Category", "Integration")]
public sealed class OutboxIntegrationTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    private static OrdersDbContext ContextFor(string connectionString) =>
        new(new DbContextOptionsBuilder<OrdersDbContext>().UseNpgsql(connectionString).Options);

    private async Task<string> FreshDatabaseAsync()
    {
        var connectionString = await TestDatabase.CreateFreshAsync(fixture.ConnectionString);
        await using var db = ContextFor(connectionString);
        await db.Database.EnsureCreatedAsync(); // esquema + seed del catálogo
        return connectionString;
    }

    private static OrderService OrderServiceFor(OrdersDbContext db) =>
        new(db, new NoOpOrderNotifier(), NullLogger<OrderService>.Instance);

    [Fact]
    public async Task Creating_an_order_writes_an_unprocessed_outbox_message_without_touching_the_broker()
    {
        var connectionString = await FreshDatabaseAsync();
        await using var db = ContextFor(connectionString);

        var result = await OrderServiceFor(db).CreateAsync(
            new CreateOrderRequest("Ada Lovelace", "ABC-01", 2), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var outbox = await db.OutboxMessages.SingleAsync();
        outbox.Type.Should().Be(nameof(OrderCreated));
        outbox.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public async Task The_processor_publishes_pending_messages_and_marks_them_processed()
    {
        var connectionString = await FreshDatabaseAsync();
        await using var db = ContextFor(connectionString);

        await OrderServiceFor(db).CreateAsync(
            new CreateOrderRequest("Ada Lovelace", "ABC-01", 2), CancellationToken.None);

        var publisher = new RecordingEventPublisher();
        var processor = new OutboxProcessor(db, publisher, NullLogger<OutboxProcessor>.Instance);

        var processed = await processor.ProcessPendingAsync(CancellationToken.None);

        processed.Should().Be(1);
        publisher.Published.Should().ContainSingle().Which.Should().BeOfType<OrderCreated>();
        (await db.OutboxMessages.SingleAsync()).ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Already_processed_messages_are_not_published_again()
    {
        var connectionString = await FreshDatabaseAsync();
        await using var db = ContextFor(connectionString);

        await OrderServiceFor(db).CreateAsync(
            new CreateOrderRequest("Ada Lovelace", "ABC-01", 2), CancellationToken.None);

        var publisher = new RecordingEventPublisher();
        var processor = new OutboxProcessor(db, publisher, NullLogger<OutboxProcessor>.Instance);

        await processor.ProcessPendingAsync(CancellationToken.None);
        await processor.ProcessPendingAsync(CancellationToken.None); // segunda pasada: nada pendiente

        publisher.Published.Should().ContainSingle();
    }
}
