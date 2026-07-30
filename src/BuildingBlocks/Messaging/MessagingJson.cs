using System.Text.Json;

namespace OrderFlow.BuildingBlocks.Messaging;

/// <summary>
/// Opciones de serialización compartidas por el publisher y los consumidores, para que el
/// contrato de los eventos (nombres de propiedades en camelCase) sea idéntico en ambos lados.
/// </summary>
public static class MessagingJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}
