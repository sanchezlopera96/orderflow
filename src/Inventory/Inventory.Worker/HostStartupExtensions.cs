using Microsoft.EntityFrameworkCore;
using OrderFlow.Inventory.Infrastructure.Persistence;

namespace OrderFlow.Inventory.Worker;

public static class HostStartupExtensions
{
    /// <summary>Aplica las migraciones pendientes de EF Core al arrancar. El seed de stock viaja en la migración.</summary>
    public static async Task MigrateInventoryDatabaseAsync(this IHost host)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
