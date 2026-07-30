using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OrderFlow.Orders.Api.Application;
using OrderFlow.Orders.Domain;
using OrderFlow.Orders.Infrastructure.Persistence;
using Xunit;

namespace OrderFlow.Orders.Tests;

public class StockResultHandlerTests
{
    private static OrdersDbContext NewDbContext() =>
        new(new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase($"orders-{Guid.NewGuid()}")
            .Options);

    private static async Task<Guid> SeedPendingOrderAsync(OrdersDbContext dbContext)
    {
        var order = Order.Create("Ada Lovelace", "ABC-01", 2);
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();
        return order.Id;
    }

    [Fact]
    public async Task Confirm_moves_a_pending_order_to_confirmed()
    {
        await using var db = NewDbContext();
        var orderId = await SeedPendingOrderAsync(db);
        var handler = new StockResultHandler(db, NullLogger<StockResultHandler>.Instance);

        await handler.ConfirmAsync(orderId, CancellationToken.None);

        (await db.Orders.SingleAsync()).Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task Reject_moves_a_pending_order_to_rejected()
    {
        await using var db = NewDbContext();
        var orderId = await SeedPendingOrderAsync(db);
        var handler = new StockResultHandler(db, NullLogger<StockResultHandler>.Instance);

        await handler.RejectAsync(orderId, "Stock insuficiente", CancellationToken.None);

        (await db.Orders.SingleAsync()).Status.Should().Be(OrderStatus.Rejected);
    }

    [Fact]
    public async Task Confirming_twice_is_idempotent()
    {
        await using var db = NewDbContext();
        var orderId = await SeedPendingOrderAsync(db);
        var handler = new StockResultHandler(db, NullLogger<StockResultHandler>.Instance);

        await handler.ConfirmAsync(orderId, CancellationToken.None);
        await handler.ConfirmAsync(orderId, CancellationToken.None);

        (await db.Orders.SingleAsync()).Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task A_conflicting_result_after_rejection_leaves_the_order_rejected()
    {
        await using var db = NewDbContext();
        var orderId = await SeedPendingOrderAsync(db);
        var handler = new StockResultHandler(db, NullLogger<StockResultHandler>.Instance);

        await handler.RejectAsync(orderId, "Stock insuficiente", CancellationToken.None);
        await handler.ConfirmAsync(orderId, CancellationToken.None); // transición ilegal: se ignora

        (await db.Orders.SingleAsync()).Status.Should().Be(OrderStatus.Rejected);
    }

    [Fact]
    public async Task A_result_for_an_unknown_order_is_ignored()
    {
        await using var db = NewDbContext();
        var handler = new StockResultHandler(db, NullLogger<StockResultHandler>.Instance);

        var act = async () => await handler.ConfirmAsync(Guid.NewGuid(), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
