using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Orders.Infrastructure.Persistence;

/// <summary>
/// Used only by the EF Core tools at design time (for example, `dotnet ef migrations add`).
/// It is never used at runtime; the application configures the context through dependency injection.
/// </summary>
public sealed class OrdersDbContextFactory : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public OrdersDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("Database__ConnectionString")
            ?? "Host=localhost;Port=5432;Database=orders;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new OrdersDbContext(options);
    }
}
