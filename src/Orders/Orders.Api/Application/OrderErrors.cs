using OrderFlow.BuildingBlocks.Results;

namespace OrderFlow.Orders.Api.Application;

public static class OrderErrors
{
    public static Error SkuNotFound(string sku) =>
        new("order.sku_not_found", $"SKU '{sku}' does not exist in the catalog.");

    public static readonly Error NotFound =
        new("order.not_found", "The requested order was not found.");
}
