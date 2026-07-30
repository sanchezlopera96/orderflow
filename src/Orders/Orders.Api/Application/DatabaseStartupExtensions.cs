using Microsoft.EntityFrameworkCore;
using OrderFlow.Orders.Infrastructure.Persistence;

namespace OrderFlow.Orders.Api.Application;

public static class DatabaseStartupExtensions
{
    /// <summary>Aplica las migraciones pendientes de EF Core al arrancar. El seed del catálogo viaja en la migración.</summary>
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
