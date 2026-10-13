namespace Pos.Ordering.Domain.Orders;

/// <summary>
/// Order lifecycle: <c>Pending</c> (waiting for stock) → <c>AwaitingPayment</c> → <c>Paid</c>,
/// or → <c>Cancelled</c> from any state before payment.
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    AwaitingPayment = 1,
    Paid = 2,
    Cancelled = 3,
}
