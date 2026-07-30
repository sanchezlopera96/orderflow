using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Inventory.Domain;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Configurations;

public sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("stock_items");

        builder.HasKey(s => s.Sku);

        builder.Property(s => s.Sku).HasMaxLength(64);
        builder.Property(s => s.Available).IsRequired();

        // Concurrencia optimista: EF incluye Version en el WHERE del UPDATE. Si dos mensajes tocan
        // el mismo SKU a la vez, uno falla con DbUpdateConcurrencyException y el handler reintenta.
        builder.Property(s => s.Version).IsConcurrencyToken();

        // Seed de stock para los mismos SKUs que el catálogo de Orders.
        // GHI-03 nace con poco stock para poder demostrar fácilmente el rechazo.
        builder.HasData(
            new { Sku = "ABC-01", Available = 100, Version = (uint)0 },
            new { Sku = "DEF-02", Available = 50, Version = (uint)0 },
            new { Sku = "GHI-03", Available = 3, Version = (uint)0 });
    }
}
