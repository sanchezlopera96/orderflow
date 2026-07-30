using FluentAssertions;
using OrderFlow.Orders.Domain;
using Xunit;

namespace OrderFlow.Orders.Tests;

public class OrderTests
{
    [Fact]
    public void Create_starts_the_order_as_pending()
    {
        var order = Order.Create("Ada Lovelace", "ABC-01", 2);

        order.Status.Should().Be(OrderStatus.Pending);
        order.Id.Should().NotBe(Guid.Empty);
        order.CustomerName.Should().Be("Ada Lovelace");
        order.Sku.Should().Be("ABC-01");
        order.Quantity.Should().Be(2);
    }

    [Fact]
    public void Confirm_moves_a_pending_order_to_confirmed()
    {
        var order = Order.Create("Ada Lovelace", "ABC-01", 1);

        order.Confirm();

        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Reject_moves_a_pending_order_to_rejected()
    {
        var order = Order.Create("Ada Lovelace", "ABC-01", 1);

        order.Reject();

        order.Status.Should().Be(OrderStatus.Rejected);
    }

    [Fact]
    public void Confirming_twice_is_an_idempotent_no_op()
    {
        var order = Order.Create("Ada Lovelace", "ABC-01", 1);

        order.Confirm();
        order.Confirm();

        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Confirming_a_rejected_order_is_an_invalid_transition()
    {
        var order = Order.Create("Ada Lovelace", "ABC-01", 1);
        order.Reject();

        var act = order.Confirm;

        act.Should().Throw<InvalidOrderStateException>();
    }

    [Fact]
    public void Rejecting_a_confirmed_order_is_an_invalid_transition()
    {
        var order = Order.Create("Ada Lovelace", "ABC-01", 1);
        order.Confirm();

        var act = order.Reject;

        act.Should().Throw<InvalidOrderStateException>();
    }
}
