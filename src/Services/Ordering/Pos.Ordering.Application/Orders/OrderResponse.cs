using Pos.Ordering.Domain.Orders;

namespace Pos.Ordering.Application.Orders;

public sealed record OrderResponse(
    Guid Id,
    string OrderNumber,
    string CashierId,
    OrderStatus Status,
    decimal Total,
    string Currency,
    DateTime CreatedOnUtc,
    DateTime? PaidOnUtc,
    string? CancellationReason,
    IReadOnlyList<OrderLineResponse> Lines);

public sealed record OrderLineResponse(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);

internal static class OrderMappings
{
    public static OrderResponse ToResponse(this Order order) => new(
        order.Id,
        order.OrderNumber,
        order.CashierId,
        order.Status,
        order.Total.Amount,
        order.Total.Currency,
        order.CreatedOnUtc,
        order.PaidOnUtc,
        order.CancellationReason,
        [.. order.Lines.Select(line => new OrderLineResponse(
            line.ProductId,
            line.ProductName,
            line.UnitPrice.Amount,
            line.Quantity,
            line.LineTotal.Amount))]);
}
