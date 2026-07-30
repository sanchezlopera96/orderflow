using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Orders.Domain;

namespace OrderFlow.Orders.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Sku);

        builder.Property(p => p.Sku).HasMaxLength(64);
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();

        // Seed del catálogo. Inventory siembra stock para estos mismos SKUs.
        builder.HasData(
            new { Sku = "ABC-01", Name = "Wireless Mouse" },
            new { Sku = "DEF-02", Name = "Mechanical Keyboard" },
            new { Sku = "GHI-03", Name = "USB-C Hub" });
    }
}
