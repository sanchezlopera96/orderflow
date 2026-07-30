using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Inventory.Domain;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Configurations;

public sealed class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
    {
        builder.ToTable("processed_events");

        // La clave primaria sobre EventId es la garantía de idempotencia a nivel de base de datos.
        builder.HasKey(e => e.EventId);

        builder.Property(e => e.ProcessedAt).IsRequired();
    }
}
