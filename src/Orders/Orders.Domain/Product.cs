namespace OrderFlow.Orders.Domain;

/// <summary>
/// Entrada del catálogo. Orders mantiene su propio catálogo (read model) para poder validar que un
/// SKU existe sin llamar de forma síncrona al servicio de Inventory.
/// </summary>
public sealed class Product
{
    // Requerido por EF Core para la materialización.
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
