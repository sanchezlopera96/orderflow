using OrderFlow.Orders.Domain;

namespace OrderFlow.Orders.Api.Application;

public sealed record OrderResponse(
    Guid Id,
    string CustomerName,
    string Sku,
    int Quantity,
    string Status,
    DateTimeOffset CreatedAt)
{
    public static OrderResponse From(Order order) =>
        new(order.Id, order.CustomerName, order.Sku, order.Quantity, order.Status.ToString(), order.CreatedAt);
}
