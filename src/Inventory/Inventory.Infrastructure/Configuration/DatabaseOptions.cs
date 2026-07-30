namespace OrderFlow.Inventory.Infrastructure.Configuration;

/// <summary>Configuración de base de datos tipada, enlazada desde configuración / variables de entorno.</summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string ConnectionString { get; init; } = string.Empty;
}
