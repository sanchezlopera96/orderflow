using FluentAssertions;
using OrderFlow.Orders.Domain;
using Xunit;

namespace OrderFlow.Orders.Tests;

public class OrderStatusTests
{
    [Fact]
    public void Order_status_defines_exactly_the_three_lifecycle_states()
    {
        Enum.GetNames<OrderStatus>()
            .Should()
            .BeEquivalentTo("Pending", "Confirmed", "Rejected");
    }

    [Fact]
    public void New_orders_conceptually_start_as_pending()
    {
        // Pending es el valor por defecto (0), así que un estado recién inicializado es Pending.
        default(OrderStatus).Should().Be(OrderStatus.Pending);
    }
}
