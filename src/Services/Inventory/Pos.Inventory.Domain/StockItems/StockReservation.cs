using Pos.BuildingBlocks.Domain.Primitives;

namespace Pos.Inventory.Domain.StockItems;

/// <summary>Stock held for an order until it is paid (committed) or cancelled (released).</summary>
public sealed class StockReservation : Entity<Guid>
{
    internal StockReservation(Guid stockItemId, Guid orderId, int quantity)
        : base(Guid.CreateVersion7())
    {
        StockItemId = stockItemId;
        OrderId = orderId;
        Quantity = quantity;
        Status = ReservationStatus.Active;
    }

    private StockReservation()
    {
    }

    public Guid StockItemId { get; private set; }

    public Guid OrderId { get; private set; }

    public int Quantity { get; private set; }

    public ReservationStatus Status { get; private set; }

    internal void Release() => Status = ReservationStatus.Released;

    internal void Commit() => Status = ReservationStatus.Committed;
}
