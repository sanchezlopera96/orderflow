using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.BuildingBlocks.Events;
using OrderFlow.BuildingBlocks.Messaging;
using OrderFlow.BuildingBlocks.Results;
using OrderFlow.Orders.Domain;
using OrderFlow.Orders.Infrastructure.Persistence;

namespace OrderFlow.Orders.Api.Application;

/// <summary>Orquestación de los casos de uso de pedidos. Los endpoints quedan delgados y delegan aquí.</summary>
public sealed class OrderService(
    OrdersDbContext dbContext,
    IEventPublisher eventPublisher,
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

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await eventPublisher.PublishAsync(
                new OrderCreated { OrderId = order.Id, Sku = order.Sku, Quantity = order.Quantity },
                cancellationToken);
        }
        catch (Exception exception)
        {
            // El pedido ya quedó persistido como Pending. Si el broker no está disponible, lo dejamos
            // en Pending y registramos el fallo en lugar de romper la petición; un outbox transaccional
            // sería la evolución robusta (ver ADR 0005).
            logger.LogError(
                exception,
                "No se pudo publicar OrderCreated para el pedido {OrderId}; queda en Pending",
                order.Id);
        }

        logger.LogInformation(
            "Pedido {OrderId} creado para el SKU {Sku} (cantidad {Quantity})",
            order.Id,
            order.Sku,
            order.Quantity);

        return Result.Success(OrderResponse.From(order));
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
