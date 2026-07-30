using FluentValidation;
using OrderFlow.Orders.Api.Application;
using OrderFlow.Orders.Api.ErrorHandling;

namespace OrderFlow.Orders.Api.Endpoints;

public static class OrdersEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders").WithTags("Orders");

        group.MapPost("/", CreateOrderAsync);
        group.MapGet("/", GetOrdersAsync);
        group.MapGet("/{id:guid}", GetOrderByIdAsync);

        return app;
    }

    private static async Task<IResult> CreateOrderAsync(
        CreateOrderRequest request,
        IValidator<CreateOrderRequest> validator,
        OrderService orderService,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(f => f.ErrorMessage).ToArray());

            return Results.ValidationProblem(errors);
        }

        var result = await orderService.CreateAsync(request, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/orders/{result.Value.Id}", result.Value)
            : result.Error.ToHttpResult();
    }

    private static async Task<IResult> GetOrdersAsync(OrderService orderService, CancellationToken cancellationToken) =>
        Results.Ok(await orderService.GetAllAsync(cancellationToken));

    private static async Task<IResult> GetOrderByIdAsync(
        Guid id,
        OrderService orderService,
        CancellationToken cancellationToken)
    {
        var result = await orderService.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.Error.ToHttpResult();
    }
}
