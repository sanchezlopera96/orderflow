using OrderFlow.Orders.Api.Application;

namespace OrderFlow.Orders.Api.Endpoints;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (ProductService productService, CancellationToken cancellationToken) =>
            Results.Ok(await productService.GetAllAsync(cancellationToken)));
    }
}
