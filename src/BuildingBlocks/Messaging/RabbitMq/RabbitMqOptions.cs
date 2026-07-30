namespace OrderFlow.BuildingBlocks.Messaging.RabbitMq;

/// <summary>Configuración del broker, enlazada desde configuración / variables de entorno.</summary>
public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; init; } = "localhost";

    public int Port { get; init; } = 5672;

    public string UserName { get; init; } = "guest";

    public string Password { get; init; } = "guest";

    public string VirtualHost { get; init; } = "/";

    public string ExchangeName { get; init; } = OrderFlowTopology.ExchangeName;

    /// <summary>Nombre visible del cliente en la consola de RabbitMQ (ayuda a diagnosticar).</summary>
    public string ClientName { get; init; } = "orderflow";
}
