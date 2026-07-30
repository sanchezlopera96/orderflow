using FluentAssertions;
using OrderFlow.BuildingBlocks.Events;
using OrderFlow.BuildingBlocks.Messaging.RabbitMq;
using Xunit;

namespace OrderFlow.Inventory.Tests.Messaging;

public class EventRoutingTests
{
    [Theory]
    [InlineData(typeof(OrderCreated), "order.created")]
    [InlineData(typeof(StockReserved), "stock.reserved")]
    [InlineData(typeof(StockRejected), "stock.rejected")]
    public void Each_event_type_maps_to_its_routing_key(Type eventType, string expectedRoutingKey)
    {
        IntegrationEventRouting.RoutingKeyFor(eventType).Should().Be(expectedRoutingKey);
    }

    [Fact]
    public void An_unregistered_event_type_throws()
    {
        var act = () => IntegrationEventRouting.RoutingKeyFor(typeof(string));

        act.Should().Throw<InvalidOperationException>();
    }
}
