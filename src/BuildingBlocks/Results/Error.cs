namespace OrderFlow.BuildingBlocks.Results;

/// <summary>A business error expressed as a stable code plus a human-readable message.</summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}
