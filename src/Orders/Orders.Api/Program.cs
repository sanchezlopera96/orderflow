var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Minimal liveness endpoint. Real health checks (Postgres + RabbitMQ) are added in a later stage.
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

// Exposed so the Orders.Api integration tests can spin up the host via WebApplicationFactory.
public partial class Program;
