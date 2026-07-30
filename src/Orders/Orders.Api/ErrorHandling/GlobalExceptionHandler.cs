using Microsoft.AspNetCore.Diagnostics;
using OrderFlow.Orders.Domain;

namespace OrderFlow.Orders.Api.ErrorHandling;

/// <summary>Convierte las excepciones no controladas en respuestas ProblemDetails consistentes.</summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Excepción no controlada al procesar {Path}", httpContext.Request.Path);

        var (statusCode, title, detail) = exception switch
        {
            InvalidOrderStateException ex => (
                StatusCodes.Status409Conflict,
                "Transición de estado inválida",
                ex.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Ocurrió un error inesperado",
                "Ocurrió un error inesperado al procesar la petición."),
        };

        await Results.Problem(detail: detail, statusCode: statusCode, title: title)
            .ExecuteAsync(httpContext);

        return true;
    }
}
