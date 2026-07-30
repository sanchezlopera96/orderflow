using Microsoft.EntityFrameworkCore;
using OrderFlow.Orders.Infrastructure.Persistence;

namespace OrderFlow.Orders.Api.Application;

public static class DatabaseStartupExtensions
{
    /// <summary>Applies pending EF Core migrations on startup. Catalog seed ships with the migration.</summary>
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
