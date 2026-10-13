using Pos.BuildingBlocks.Domain.ValueObjects;
using Pos.Ordering.Domain.Orders;
using Pos.Ordering.Domain.Orders.Events;

namespace Pos.Ordering.UnitTests.Domain;

public sealed class OrderTests
{
    private static readonly DateTime Now = new(2026, 10, 13, 9, 30, 0, DateTimeKind.Utc);
    private static readonly Guid Espresso = Guid.NewGuid();
    private static readonly Guid Croissant = Guid.NewGuid();

    [Fact]
    public void Place_WithValidLines_CalculatesTotalAndRaisesOrderPlaced()
    {
        Order order = Order.Place("cashier-1", [Line(Espresso, 2.50m, 2), Line(Croissant, 2.95m, 1)], Now).Value;

        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(7.95m, order.Total.Amount);
        Assert.Equal("USD", order.Total.Currency);
        Assert.StartsWith("POS-20261013-", order.OrderNumber, StringComparison.Ordinal);
        var placed = Assert.IsType<OrderPlacedDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(2, placed.Items.Count);
    }

    [Fact]
    public void Place_WithDuplicateProducts_MergesLines()
    {
        Order order = Order.Place("cashier-1", [Line(Espresso, 2.50m, 1), Line(Espresso, 2.50m, 2)], Now).Value;

        OrderLine line = Assert.Single(order.Lines);
        Assert.Equal(3, line.Quantity);
        Assert.Equal(7.50m, order.Total.Amount);
    }

    [Fact]
    public void Place_WithoutLines_ReturnsFailure()
    {
        var result = Order.Place("cashier-1", [], Now);

        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.InvalidLineCount, result.Error);
    }

    [Fact]
    public void Place_WithMixedCurrencies_ReturnsFailure()
    {
        OrderLineDraft euro = new(Croissant, "Croissant", Money.Create(2m, "EUR").Value, 1);

        var result = Order.Place("cashier-1", [Line(Espresso, 2.50m, 1), euro], Now);

        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.MixedCurrencies, result.Error);
    }

    [Fact]
    public void MarkAwaitingPayment_FromPending_RaisesReadyForPaymentEvent()
    {
        Order order = PendingOrder();

        Assert.True(order.MarkAwaitingPayment().IsSuccess);

        Assert.Equal(OrderStatus.AwaitingPayment, order.Status);
        Assert.Single(order.DomainEvents.OfType<OrderAwaitingPaymentDomainEvent>());
    }

    [Fact]
    public void MarkPaid_WhenStillPending_ReturnsInvalidTransition()
    {
        var result = PendingOrder().MarkPaid(Guid.NewGuid(), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Order.InvalidTransition", result.Error.Code);
    }

    [Fact]
    public void MarkPaid_WhenAwaitingPayment_CompletesOrder()
    {
        Order order = PendingOrder();
        order.MarkAwaitingPayment();
        Guid paymentId = Guid.NewGuid();

        Assert.True(order.MarkPaid(paymentId, Now).IsSuccess);

        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal(paymentId, order.PaymentId);
        Assert.Equal(Now, order.PaidOnUtc);
    }

    [Fact]
    public void Cancel_PaidOrder_ReturnsInvalidTransition()
    {
        Order order = PendingOrder();
        order.MarkAwaitingPayment();
        order.MarkPaid(Guid.NewGuid(), Now);

        Assert.True(order.Cancel("Customer changed mind").IsFailure);
    }

    [Fact]
    public void Cancel_IsIdempotentAndRaisesEventOnce()
    {
        Order order = PendingOrder();

        order.Cancel("Out of stock");
        order.Cancel("Out of stock");

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Single(order.DomainEvents.OfType<OrderCancelledDomainEvent>());
    }

    private static OrderLineDraft Line(Guid productId, decimal price, int quantity) =>
        new(productId, "Item", Money.Create(price, "USD").Value, quantity);

    private static Order PendingOrder()
    {
        Order order = Order.Place("cashier-1", [Line(Espresso, 2.50m, 1)], Now).Value;
        order.ClearDomainEvents();
        return order;
    }
}
