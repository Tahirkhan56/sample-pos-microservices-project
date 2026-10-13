using Pos.BuildingBlocks.Domain.Primitives;
using Pos.BuildingBlocks.Domain.Results;
using Pos.BuildingBlocks.Domain.ValueObjects;
using Pos.Ordering.Domain.Orders.Events;

namespace Pos.Ordering.Domain.Orders;

/// <summary>A sale rung up at the till. Drives the order saga through its state transitions.</summary>
public sealed class Order : AggregateRoot<Guid>
{
    public const int OrderNumberMaxLength = 32;
    public const int CashierIdMaxLength = 64;
    public const int ReasonMaxLength = 500;
    public const int MaxLines = 100;

    private readonly List<OrderLine> _lines = [];

    private Order(Guid id, string orderNumber, string cashierId, DateTime createdOnUtc)
        : base(id)
    {
        OrderNumber = orderNumber;
        CashierId = cashierId;
        CreatedOnUtc = createdOnUtc;
        Status = OrderStatus.Pending;
    }

    private Order()
    {
    }

    public string OrderNumber { get; private set; } = string.Empty;

    public string CashierId { get; private set; } = string.Empty;

    public OrderStatus Status { get; private set; }

    public Money Total { get; private set; } = null!;

    public DateTime CreatedOnUtc { get; private set; }

    public DateTime? PaidOnUtc { get; private set; }

    public Guid? PaymentId { get; private set; }

    public string? CancellationReason { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    public static Result<Order> Place(string cashierId, IReadOnlyCollection<OrderLineDraft> drafts, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(drafts);

        if (string.IsNullOrWhiteSpace(cashierId))
        {
            return OrderErrors.CashierRequired;
        }

        if (drafts.Count == 0 || drafts.Count > MaxLines)
        {
            return OrderErrors.InvalidLineCount;
        }

        if (drafts.Any(draft => draft.Quantity <= 0))
        {
            return OrderErrors.InvalidQuantity;
        }

        string currency = drafts.First().UnitPrice.Currency;
        if (drafts.Any(draft => !string.Equals(draft.UnitPrice.Currency, currency, StringComparison.Ordinal)))
        {
            return OrderErrors.MixedCurrencies;
        }

        Guid id = Guid.CreateVersion7();
        var order = new Order(id, GenerateOrderNumber(id, utcNow), cashierId.Trim(), utcNow);

        // Merge duplicate scans of the same product into one line.
        foreach (IGrouping<Guid, OrderLineDraft> group in drafts.GroupBy(draft => draft.ProductId))
        {
            OrderLineDraft first = group.First();
            order._lines.Add(new OrderLine(id, first.ProductId, first.ProductName, first.UnitPrice, group.Sum(d => d.Quantity)));
        }

        order.Total = order._lines.Aggregate(Money.Zero(currency), (sum, line) => sum + line.LineTotal);
        order.Raise(new OrderPlacedDomainEvent(
            order.Id,
            [.. order._lines.Select(line => new OrderPlacedDomainEvent.Item(line.ProductId, line.Quantity))]));

        return order;
    }

    /// <summary>Stock is reserved; the order can now be paid.</summary>
    public Result MarkAwaitingPayment()
    {
        if (Status == OrderStatus.AwaitingPayment)
        {
            return Result.Success();
        }

        if (Status != OrderStatus.Pending)
        {
            return OrderErrors.InvalidTransition(Status, OrderStatus.AwaitingPayment);
        }

        Status = OrderStatus.AwaitingPayment;
        Raise(new OrderAwaitingPaymentDomainEvent(Id, OrderNumber, Total));
        return Result.Success();
    }

    public Result MarkPaid(Guid paymentId, DateTime utcNow)
    {
        if (Status == OrderStatus.Paid)
        {
            return Result.Success();
        }

        if (Status != OrderStatus.AwaitingPayment)
        {
            return OrderErrors.InvalidTransition(Status, OrderStatus.Paid);
        }

        Status = OrderStatus.Paid;
        PaymentId = paymentId;
        PaidOnUtc = utcNow;
        return Result.Success();
    }

    public Result Cancel(string reason)
    {
        if (Status == OrderStatus.Cancelled)
        {
            return Result.Success();
        }

        if (Status == OrderStatus.Paid)
        {
            return OrderErrors.InvalidTransition(Status, OrderStatus.Cancelled);
        }

        Status = OrderStatus.Cancelled;
        CancellationReason = reason.Length > ReasonMaxLength ? reason[..ReasonMaxLength] : reason;
        Raise(new OrderCancelledDomainEvent(Id, CancellationReason));
        return Result.Success();
    }

    private static string GenerateOrderNumber(Guid id, DateTime utcNow) =>
        $"POS-{utcNow:yyyyMMdd}-{id.ToString("N")[^8..].ToUpperInvariant()}";
}
