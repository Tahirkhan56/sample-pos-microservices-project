using Pos.BuildingBlocks.Domain.Results;

namespace Pos.Ordering.Domain.Orders;

public static class OrderErrors
{
    public static readonly Error CashierRequired = Error.Validation("Order.CashierRequired", "Cashier id is required.");

    public static readonly Error InvalidLineCount =
        Error.Validation("Order.InvalidLineCount", $"An order must have between 1 and {Order.MaxLines} lines.");

    public static readonly Error InvalidQuantity = Error.Validation("Order.InvalidQuantity", "Quantities must be greater than zero.");

    public static readonly Error MixedCurrencies = Error.Validation("Order.MixedCurrencies", "All lines must use the same currency.");

    public static Error NotFound(Guid orderId) => Error.NotFound("Order.NotFound", $"Order '{orderId}' was not found.");

    public static Error InvalidTransition(OrderStatus from, OrderStatus to) =>
        Error.Conflict("Order.InvalidTransition", $"Cannot move an order from {from} to {to}.");
}
