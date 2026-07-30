using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// The RabbitMQ consumer (a BackgroundService) is registered here in a later stage.
var host = builder.Build();

host.Run();
