using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Orders.Api.Configuration;

/// <summary>Configuración de base de datos tipada, enlazada desde configuración / variables de entorno.</summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required(AllowEmptyStrings = false)]
    public string ConnectionString { get; init; } = string.Empty;
}
