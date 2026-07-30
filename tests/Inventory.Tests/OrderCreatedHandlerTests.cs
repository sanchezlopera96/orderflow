using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OrderFlow.BuildingBlocks.Events;
using OrderFlow.Inventory.Domain;
using OrderFlow.Inventory.Infrastructure.Messaging;
using OrderFlow.Inventory.Infrastructure.Persistence;
using OrderFlow.Inventory.Tests.Support;
using Xunit;

namespace OrderFlow.Inventory.Tests;

public class OrderCreatedHandlerTests
{
    private static InventoryDbContext NewDbContext() =>
        new(new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase($"inventory-{Guid.NewGuid()}")
            .Options);

    [Fact]
    public async Task Handling_an_order_with_enough_stock_reserves_it_and_publishes_stock_reserved()
    {
        await using var db = NewDbContext();
        db.StockItems.Add(new StockItem("ABC-01", 10));
        await db.SaveChangesAsync();

        var publisher = new RecordingEventPublisher();
        var handler = new OrderCreatedHandler(db, publisher, NullLogger<OrderCreatedHandler>.Instance);
        var message = new OrderCreated { OrderId = Guid.NewGuid(), Sku = "ABC-01", Quantity = 3 };

        await handler.HandleAsync(message, CancellationToken.None);

        (await db.StockItems.SingleAsync()).Available.Should().Be(7);
        publisher.Published.Should().ContainSingle().Which.Should().BeOfType<StockReserved>();
    }

    [Fact]
    public async Task Processing_the_same_event_twice_decrements_stock_only_once()
    {
        await using var db = NewDbContext();
        db.StockItems.Add(new StockItem("ABC-01", 10));
        await db.SaveChangesAsync();

        var publisher = new RecordingEventPublisher();
        var handler = new OrderCreatedHandler(db, publisher, NullLogger<OrderCreatedHandler>.Instance);
        var message = new OrderCreated { OrderId = Guid.NewGuid(), Sku = "ABC-01", Quantity = 4 };

        await handler.HandleAsync(message, CancellationToken.None);
        await handler.HandleAsync(message, CancellationToken.None);

        (await db.StockItems.SingleAsync()).Available.Should().Be(6);
        publisher.Published.Should().ContainSingle();
    }

    [Fact]
    public async Task Handling_an_order_without_enough_stock_publishes_stock_rejected_and_keeps_stock()
    {
        await using var db = NewDbContext();
        db.StockItems.Add(new StockItem("GHI-03", 3));
        await db.SaveChangesAsync();

        var publisher = new RecordingEventPublisher();
        var handler = new OrderCreatedHandler(db, publisher, NullLogger<OrderCreatedHandler>.Instance);
        var message = new OrderCreated { OrderId = Guid.NewGuid(), Sku = "GHI-03", Quantity = 5 };

        await handler.HandleAsync(message, CancellationToken.None);

        (await db.StockItems.SingleAsync()).Available.Should().Be(3);
        publisher.Published.Should().ContainSingle().Which.Should().BeOfType<StockRejected>();
    }

    [Fact]
    public async Task Handling_an_order_for_an_unknown_sku_publishes_stock_rejected()
    {
        await using var db = NewDbContext();

        var publisher = new RecordingEventPublisher();
        var handler = new OrderCreatedHandler(db, publisher, NullLogger<OrderCreatedHandler>.Instance);
        var message = new OrderCreated { OrderId = Guid.NewGuid(), Sku = "ZZZ-99", Quantity = 1 };

        await handler.HandleAsync(message, CancellationToken.None);

        publisher.Published.Should().ContainSingle().Which.Should().BeOfType<StockRejected>();
    }
}
