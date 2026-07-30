namespace OrderFlow.Orders.Api.Application;

public sealed record CreateOrderRequest(string CustomerName, string Sku, int Quantity);
