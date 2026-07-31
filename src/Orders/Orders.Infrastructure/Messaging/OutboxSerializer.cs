using System.Text.Json;
using OrderFlow.BuildingBlocks.Events;
using OrderFlow.BuildingBlocks.Messaging;
using OrderFlow.Orders.Infrastructure.Persistence;

namespace OrderFlow.Orders.Infrastructure.Messaging;

/// <summary>Reconstruye el evento de integración a partir de un <see cref="OutboxMessage"/>.</summary>
public static class OutboxSerializer
{
    private static readonly IReadOnlyDictionary<string, Type> KnownTypes = new Dictionary<string, Type>
    {
        [nameof(OrderCreated)] = typeof(OrderCreated),
    };

    public static IntegrationEvent Deserialize(OutboxMessage message)
    {
        if (!KnownTypes.TryGetValue(message.Type, out var type))
        {
            throw new InvalidOperationException($"Tipo de evento desconocido en el outbox: {message.Type}");
        }

        return (IntegrationEvent)JsonSerializer.Deserialize(message.Content, type, MessagingJson.Options)!;
    }
}
