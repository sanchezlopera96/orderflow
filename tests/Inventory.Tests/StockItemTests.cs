using FluentAssertions;
using OrderFlow.Inventory.Domain;
using Xunit;

namespace OrderFlow.Inventory.Tests;

public class StockItemTests
{
    [Fact]
    public void Reserve_reduces_availability_when_there_is_enough_stock()
    {
        var stock = new StockItem("ABC-01", 10);

        var outcome = stock.Reserve(3);

        outcome.Should().Be(ReservationOutcome.Reserved);
        stock.Available.Should().Be(7);
    }

    [Fact]
    public void Reserve_the_exact_available_quantity_succeeds()
    {
        var stock = new StockItem("ABC-01", 5);

        var outcome = stock.Reserve(5);

        outcome.Should().Be(ReservationOutcome.Reserved);
        stock.Available.Should().Be(0);
    }

    [Fact]
    public void Reserve_more_than_available_leaves_stock_untouched()
    {
        var stock = new StockItem("ABC-01", 2);

        var outcome = stock.Reserve(3);

        outcome.Should().Be(ReservationOutcome.InsufficientStock);
        stock.Available.Should().Be(2);
    }

    [Fact]
    public void Reserve_a_non_positive_quantity_is_rejected()
    {
        var stock = new StockItem("ABC-01", 10);

        var act = () => stock.Reserve(0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
