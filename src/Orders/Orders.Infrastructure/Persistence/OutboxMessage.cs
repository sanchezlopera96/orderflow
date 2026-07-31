using System.Text.Json;
using OrderFlow.BuildingBlocks.Events;
using OrderFlow.BuildingBlocks.Messaging;

namespace OrderFlow.Orders.Infrastructure.Persistence;

/// <summary>
/// Mensaje del outbox transaccional. Al crear un pedido, el evento se guarda aquí en la misma
/// transacción que el pedido; un despachador aparte lo publica luego. Así el evento nunca se pierde
/// aunque el broker esté caído en el momento de crear el pedido.
/// </summary>
public sealed class OutboxMessage
{
    // Requerido por EF Core.
    private OutboxMessage()
    {
    }

    private OutboxMessage(Guid id, string type, string content, DateTimeOffset occurredAt)
    {
        Id = id;
        Type = type;
        Content = content;
        OccurredAt = occurredAt;
    }

    public Guid Id { get; private set; }

    /// <summary>Nombre del tipo del evento (para reconstruirlo al despachar).</summary>
    public string Type { get; private set; } = null!;

    /// <summary>Evento serializado en JSON.</summary>
    public string Content { get; private set; } = null!;

    public DateTimeOffset OccurredAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public int Attempts { get; private set; }

    public string? Error { get; private set; }

    public static OutboxMessage For(IntegrationEvent integrationEvent)
    {
        var type = integrationEvent.GetType();
        var content = JsonSerializer.Serialize(integrationEvent, type, MessagingJson.Options);
        return new OutboxMessage(Guid.NewGuid(), type.Name, content, integrationEvent.OccurredAt);
    }

    public void MarkProcessed()
    {
        ProcessedAt = DateTimeOffset.UtcNow;
        Error = null;
    }

    public void RecordFailure(string error)
    {
        Attempts++;
        Error = error;
    }
}
