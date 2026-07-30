using System.Text.Json;
using FluentAssertions;
using OrderFlow.BuildingBlocks.Events;
using OrderFlow.BuildingBlocks.Messaging;
using Xunit;

namespace OrderFlow.Inventory.Tests.Messaging;

public class EventSerializationTests
{
    [Fact]
    public void An_order_created_event_survives_a_json_round_trip()
    {
        var original = new OrderCreated { OrderId = Guid.NewGuid(), Sku = "ABC-01", Quantity = 3 };

        var json = JsonSerializer.Serialize(original, MessagingJson.Options);
        var restored = JsonSerializer.Deserialize<OrderCreated>(json, MessagingJson.Options);

        restored.Should().NotBeNull();
        restored!.EventId.Should().Be(original.EventId);
        restored.OrderId.Should().Be(original.OrderId);
        restored.Sku.Should().Be(original.Sku);
        restored.Quantity.Should().Be(original.Quantity);
        restored.OccurredAt.Should().Be(original.OccurredAt);
    }

    [Fact]
    public void The_json_contract_uses_camel_case_property_names()
    {
        var @event = new OrderCreated { OrderId = Guid.NewGuid(), Sku = "ABC-01", Quantity = 1 };

        var json = JsonSerializer.Serialize(@event, MessagingJson.Options);

        json.Should().Contain("\"orderId\"").And.Contain("\"eventId\"").And.Contain("\"quantity\"");
    }
}
