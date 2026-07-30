using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderFlow.BuildingBlocks.Messaging.RabbitMq;
using OrderFlow.Inventory.Infrastructure.Configuration;
using OrderFlow.Inventory.Infrastructure.Messaging;
using OrderFlow.Inventory.Infrastructure.Persistence;
using OrderFlow.Inventory.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection(DatabaseOptions.SectionName))
    .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString), "Database:ConnectionString es obligatorio.")
    .ValidateOnStart();

builder.Services.AddDbContext<InventoryDbContext>((serviceProvider, options) =>
{
    var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    options.UseNpgsql(databaseOptions.ConnectionString);
});

builder.Services.AddRabbitMqMessaging(builder.Configuration);
builder.Services.AddScoped<OrderCreatedHandler>();
builder.Services.AddHostedService<OrderCreatedConsumer>();

var host = builder.Build();

await host.MigrateInventoryDatabaseAsync();

await host.RunAsync();
