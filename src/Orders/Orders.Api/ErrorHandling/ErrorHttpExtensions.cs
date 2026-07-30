using OrderFlow.BuildingBlocks.Results;

namespace OrderFlow.Orders.Api.ErrorHandling;

public static class ErrorHttpExtensions
{
    /// <summary>Maps a business <see cref="Error"/> to a ProblemDetails HTTP result.</summary>
    public static IResult ToHttpResult(this Error error)
    {
        var statusCode = error.Code.EndsWith("not_found", StringComparison.Ordinal)
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status400BadRequest;

        return Results.Problem(detail: error.Message, statusCode: statusCode, title: error.Code);
    }
}
