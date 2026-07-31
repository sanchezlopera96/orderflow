using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.BuildingBlocks.Events;
using OrderFlow.BuildingBlocks.Results;
using OrderFlow.Orders.Api.Realtime;
using OrderFlow.Orders.Domain;
using OrderFlow.Orders.Infrastructure.Persistence;

namespace OrderFlow.Orders.Api.Application;

/// <summary>Orquestación de los casos de uso de pedidos. Los endpoints quedan delgados y delegan aquí.</summary>
public sealed class OrderService(
    OrdersDbContext dbContext,
    IOrderNotifier orderNotifier,
    ILogger<OrderService> logger)
{
    public async Task<Result<OrderResponse>> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var skuExists = await dbContext.Products
            .AsNoTracking()
            .AnyAsync(p => p.Sku == request.Sku, cancellationToken);

        if (!skuExists)
        {
            return Result.Failure<OrderResponse>(OrderErrors.SkuNotFound(request.Sku));
        }

        var order = Order.Create(request.CustomerName, request.Sku, request.Quantity);

        // El pedido y el evento se guardan en la MISMA transacción (outbox transaccional): así el
        // evento nunca se pierde, aunque el broker esté caído. Un despachador aparte lo publica luego.
        dbContext.Orders.Add(order);
        dbContext.OutboxMessages.Add(
            OutboxMessage.For(new OrderCreated { OrderId = order.Id, Sku = order.Sku, Quantity = order.Quantity }));
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = OrderResponse.From(order);
        await orderNotifier.OrderChangedAsync(response, cancellationToken);

        logger.LogInformation(
            "Pedido {OrderId} creado para el SKU {Sku} (cantidad {Quantity})",
            order.Id,
            order.Sku,
            order.Quantity);

        return Result.Success(response);
    }

    public async Task<IReadOnlyList<OrderResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await dbContext.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(OrderResponse.From).ToList();
    }

    public async Task<Result<OrderResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return order is null
            ? Result.Failure<OrderResponse>(OrderErrors.NotFound)
            : Result.Success(OrderResponse.From(order));
    }
}
