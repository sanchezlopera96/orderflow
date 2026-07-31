using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderFlow.BuildingBlocks.Messaging.RabbitMq;
using OrderFlow.Orders.Api.Application;
using OrderFlow.Orders.Api.Application.Validation;
using OrderFlow.Orders.Api.Configuration;
using OrderFlow.Orders.Api.Endpoints;
using OrderFlow.Orders.Api.ErrorHandling;
using OrderFlow.Orders.Api.Messaging;
using OrderFlow.Orders.Api.Realtime;
using OrderFlow.Orders.Infrastructure.Messaging;
using OrderFlow.Orders.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services
    .AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection(DatabaseOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddDbContext<OrdersDbContext>((serviceProvider, options) =>
{
    var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    options.UseNpgsql(databaseOptions.ConnectionString);
});

builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();
builder.Services.AddScoped<OrderService>();

// Publisher real de RabbitMQ (reemplaza al no-op de la etapa anterior).
builder.Services.AddRabbitMqMessaging(builder.Configuration);

// Consumidor de resultados de stock: confirma o rechaza el pedido.
builder.Services.AddScoped<StockResultHandler>();
builder.Services.AddHostedService<StockResultConsumer>();

// Outbox transaccional: el evento se guarda con el pedido y este despachador lo publica.
builder.Services.AddScoped<OutboxProcessor>();
builder.Services.AddHostedService<OutboxDispatcher>();

// Tiempo real: empuja los cambios de pedido a los clientes por SignalR.
builder.Services
    .AddSignalR()
    .AddJsonProtocol(options =>
        options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);
builder.Services.AddSingleton<IOrderNotifier, SignalROrderNotifier>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrdersDbContext>("orders-db")
    .AddCheck<RabbitMqHealthCheck>("rabbitmq");

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");
app.MapHub<OrdersHub>("/hubs/orders");
app.MapOrderEndpoints();

await app.ApplyMigrationsAsync();

app.Run();

// Se expone para que futuras pruebas de integración puedan levantar el host con WebApplicationFactory.
public partial class Program;
