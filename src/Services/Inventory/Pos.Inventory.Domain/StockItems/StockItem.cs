using Pos.BuildingBlocks.Domain.Primitives;
using Pos.BuildingBlocks.Domain.Results;

namespace Pos.Inventory.Domain.StockItems;

/// <summary>
/// Stock level for one product. Enforces the invariant <c>0 &lt;= Reserved &lt;= OnHand</c>.
/// <see cref="Version"/> is an optimistic-concurrency token so concurrent reservations cannot oversell.
/// </summary>
public sealed class StockItem : AggregateRoot<Guid>
{
    public const int SkuMaxLength = 32;
    public const int NameMaxLength = 200;

    private readonly List<StockReservation> _reservations = [];

    private StockItem(Guid id, Guid productId, string sku, string productName)
        : base(id)
    {
        ProductId = productId;
        Sku = sku;
        ProductName = productName;
    }

    private StockItem()
    {
    }

    public Guid ProductId { get; private set; }

    public string Sku { get; private set; } = string.Empty;

    public string ProductName { get; private set; } = string.Empty;

    public int QuantityOnHand { get; private set; }

    public int QuantityReserved { get; private set; }

    public int QuantityAvailable => QuantityOnHand - QuantityReserved;

    public int Version { get; private set; }

    /// <summary>Active reservations (repositories load only active ones).</summary>
    public IReadOnlyCollection<StockReservation> Reservations => _reservations.AsReadOnly();

    public static StockItem Create(Guid productId, string sku, string productName) =>
        new(Guid.CreateVersion7(), productId, sku, productName);

    public Result Receive(int quantity)
    {
        if (quantity <= 0)
        {
            return StockItemErrors.InvalidQuantity;
        }

        QuantityOnHand += quantity;
        Version++;
        return Result.Success();
    }

    public bool CanReserve(int quantity) => quantity > 0 && quantity <= QuantityAvailable;

    public Result Reserve(Guid orderId, int quantity)
    {
        if (quantity <= 0)
        {
            return StockItemErrors.InvalidQuantity;
        }

        if (FindActiveReservation(orderId) is not null)
        {
            return Result.Success(); // Idempotent: this order already holds stock.
        }

        if (quantity > QuantityAvailable)
        {
            return StockItemErrors.InsufficientStock(ProductId, quantity, QuantityAvailable);
        }

        _reservations.Add(new StockReservation(Id, orderId, quantity));
        QuantityReserved += quantity;
        Version++;
        return Result.Success();
    }

    public void Release(Guid orderId)
    {
        StockReservation? reservation = FindActiveReservation(orderId);
        if (reservation is null)
        {
            return;
        }

        reservation.Release();
        QuantityReserved -= reservation.Quantity;
        Version++;
    }

    public void Commit(Guid orderId)
    {
        StockReservation? reservation = FindActiveReservation(orderId);
        if (reservation is null)
        {
            return;
        }

        reservation.Commit();
        QuantityReserved -= reservation.Quantity;
        QuantityOnHand -= reservation.Quantity;
        Version++;
    }

    private StockReservation? FindActiveReservation(Guid orderId) =>
        _reservations.Find(r => r.OrderId == orderId && r.Status == ReservationStatus.Active);
}
