using Microsoft.EntityFrameworkCore;
using OrderFlow.Orders.Infrastructure.Persistence;

namespace OrderFlow.Orders.Api.Application;

/// <summary>Consulta del catálogo de productos (read model de Orders).</summary>
public sealed class ProductService(OrdersDbContext dbContext)
{
    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await dbContext.Products
            .AsNoTracking()
            .OrderBy(p => p.Sku)
            .ToListAsync(cancellationToken);

        return products.Select(ProductResponse.From).ToList();
    }
}
