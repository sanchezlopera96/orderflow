namespace OrderFlow.Inventory.Domain;

/// <summary>
/// Registro del inbox de idempotencia: un evento ya procesado, identificado por su EventId.
/// La clave primaria sobre EventId es la que garantiza que un evento no se procese dos veces.
/// </summary>
public sealed class ProcessedEvent
{
    // Requerido por EF Core para la materialización.
    private ProcessedEvent()
    {
    }

    public ProcessedEvent(Guid eventId)
    {
        EventId = eventId;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    public Guid EventId { get; private set; }

    public DateTimeOffset ProcessedAt { get; private set; }
}
