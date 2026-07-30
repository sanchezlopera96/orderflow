using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Orders.Api.Configuration;

/// <summary>Strongly-typed database settings, bound from configuration / environment variables.</summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required(AllowEmptyStrings = false)]
    public string ConnectionString { get; init; } = string.Empty;
}
