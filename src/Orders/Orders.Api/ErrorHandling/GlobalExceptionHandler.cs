using Microsoft.AspNetCore.Diagnostics;
using OrderFlow.Orders.Domain;

namespace OrderFlow.Orders.Api.ErrorHandling;

/// <summary>Converts unhandled exceptions into consistent ProblemDetails responses.</summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception while processing {Path}", httpContext.Request.Path);

        var (statusCode, title, detail) = exception switch
        {
            InvalidOrderStateException ex => (
                StatusCodes.Status409Conflict,
                "Invalid order state transition",
                ex.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                "An unexpected error occurred while processing the request."),
        };

        await Results.Problem(detail: detail, statusCode: statusCode, title: title)
            .ExecuteAsync(httpContext);

        return true;
    }
}
