namespace OrderFlow.BuildingBlocks.Results;

/// <summary>Un error de negocio expresado como un código estable más un mensaje legible.</summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}
