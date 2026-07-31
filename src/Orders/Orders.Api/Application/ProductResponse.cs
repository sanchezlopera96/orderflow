using OrderFlow.Orders.Domain;

namespace OrderFlow.Orders.Api.Application;

/// <summary>Entrada del catálogo expuesta por la API.</summary>
public sealed record ProductResponse(string Sku, string Name)
{
    public static ProductResponse From(Product product) => new(product.Sku, product.Name);
}
