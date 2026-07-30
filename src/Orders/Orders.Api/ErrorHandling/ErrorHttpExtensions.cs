using OrderFlow.BuildingBlocks.Results;

namespace OrderFlow.Orders.Api.ErrorHandling;

public static class ErrorHttpExtensions
{
    /// <summary>Mapea un <see cref="Error"/> de negocio a un resultado HTTP con ProblemDetails.</summary>
    public static IResult ToHttpResult(this Error error)
    {
        var statusCode = error.Code.EndsWith("not_found", StringComparison.Ordinal)
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status400BadRequest;

        return Results.Problem(detail: error.Message, statusCode: statusCode, title: error.Code);
    }
}
