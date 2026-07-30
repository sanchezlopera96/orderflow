using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// El consumidor de RabbitMQ (un BackgroundService) se registra aquí en una etapa posterior.
var host = builder.Build();

host.Run();
