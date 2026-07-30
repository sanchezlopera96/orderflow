using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderFlow.BuildingBlocks.Messaging.RabbitMq;
using OrderFlow.Orders.Api.Application;
using OrderFlow.Orders.Api.Application.Validation;
using OrderFlow.Orders.Api.Configuration;
using OrderFlow.Orders.Api.Endpoints;
using OrderFlow.Orders.Api.ErrorHandling;
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

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapOrderEndpoints();

await app.ApplyMigrationsAsync();

app.Run();

// Se expone para que futuras pruebas de integración puedan levantar el host con WebApplicationFactory.
public partial class Program;
