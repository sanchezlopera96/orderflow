using FluentAssertions;
using OrderFlow.BuildingBlocks.Events;
using Xunit;

namespace OrderFlow.Inventory.Tests;

public class IntegrationEventTests
{
    [Fact]
    public void Integration_event_assigns_identity_and_timestamp_by_default()
    {
        var @event = new OrderCreated
        {
            OrderId = Guid.NewGuid(),
            Sku = "ABC-01",
            Quantity = 2,
        };

        @event.EventId.Should().NotBe(Guid.Empty);
        @event.OccurredAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Event_id_is_the_idempotency_key_and_is_preserved_when_copied()
    {
        var original = new OrderCreated { OrderId = Guid.NewGuid(), Sku = "ABC-01", Quantity = 1 };

        var redelivered = original with { };

        redelivered.EventId.Should().Be(original.EventId);
    }
}
