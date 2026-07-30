namespace OrderFlow.Orders.Domain;

/// <summary>
/// A catalog entry. Orders keeps its own catalog (read model) so it can validate that a SKU exists
/// without calling the Inventory service synchronously.
/// </summary>
public sealed class Product
{
    // Required by EF Core for materialization.
    private Product()
    {
    }

    public Product(string sku, string name)
    {
        Sku = sku;
        Name = name;
    }

    public string Sku { get; private set; } = null!;

    public string Name { get; private set; } = null!;
}
