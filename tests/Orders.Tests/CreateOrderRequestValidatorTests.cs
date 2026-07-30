using FluentAssertions;
using FluentValidation.TestHelper;
using OrderFlow.Orders.Api.Application;
using OrderFlow.Orders.Api.Application.Validation;
using Xunit;

namespace OrderFlow.Orders.Tests;

public class CreateOrderRequestValidatorTests
{
    private readonly CreateOrderRequestValidator _validator = new();

    [Fact]
    public void A_well_formed_request_is_valid()
    {
        var request = new CreateOrderRequest("Ada Lovelace", "ABC-01", 2);

        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void An_empty_customer_name_is_invalid()
    {
        var request = new CreateOrderRequest("", "ABC-01", 2);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.CustomerName);
    }

    [Fact]
    public void An_empty_sku_is_invalid()
    {
        var request = new CreateOrderRequest("Ada Lovelace", "", 2);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Sku);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void A_quantity_outside_one_to_hundred_is_invalid(int quantity)
    {
        var request = new CreateOrderRequest("Ada Lovelace", "ABC-01", quantity);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void The_quantity_boundaries_are_valid(int quantity)
    {
        var request = new CreateOrderRequest("Ada Lovelace", "ABC-01", quantity);

        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }
}
