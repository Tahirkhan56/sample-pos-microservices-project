using Pos.BuildingBlocks.Domain.Primitives;
using Pos.BuildingBlocks.Domain.ValueObjects;

namespace Pos.Ordering.Domain.Orders;

public sealed class OrderLine : Entity<Guid>
{
    public const int ProductNameMaxLength = 200;

    internal OrderLine(Guid orderId, Guid productId, string productName, Money unitPrice, int quantity)
        : base(Guid.CreateVersion7())
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    private OrderLine()
    {
    }

    public Guid OrderId { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } = string.Empty;

    public Money UnitPrice { get; private set; } = null!;

    public int Quantity { get; private set; }

    public Money LineTotal => UnitPrice.Multiply(Quantity);
}
