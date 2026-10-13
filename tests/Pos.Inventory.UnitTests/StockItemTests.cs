using Pos.Inventory.Domain.StockItems;

namespace Pos.Inventory.UnitTests;

public sealed class StockItemTests
{
    private static readonly Guid OrderId = Guid.NewGuid();

    [Fact]
    public void Receive_PositiveQuantity_IncreasesOnHandAndVersion()
    {
        StockItem item = NewItem();

        var result = item.Receive(10);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, item.QuantityOnHand);
        Assert.Equal(10, item.QuantityAvailable);
        Assert.Equal(1, item.Version);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Receive_NonPositiveQuantity_ReturnsFailure(int quantity)
    {
        var result = NewItem().Receive(quantity);

        Assert.True(result.IsFailure);
        Assert.Equal(StockItemErrors.InvalidQuantity, result.Error);
    }

    [Fact]
    public void Reserve_WithinAvailable_ReducesAvailableButNotOnHand()
    {
        StockItem item = ItemWithStock(10);

        var result = item.Reserve(OrderId, 4);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, item.QuantityOnHand);
        Assert.Equal(4, item.QuantityReserved);
        Assert.Equal(6, item.QuantityAvailable);
        Assert.Single(item.Reservations);
    }

    [Fact]
    public void Reserve_MoreThanAvailable_ReturnsConflict()
    {
        StockItem item = ItemWithStock(3);

        var result = item.Reserve(OrderId, 4);

        Assert.True(result.IsFailure);
        Assert.Equal("Stock.Insufficient", result.Error.Code);
        Assert.Equal(0, item.QuantityReserved);
    }

    [Fact]
    public void Reserve_SameOrderTwice_IsIdempotent()
    {
        StockItem item = ItemWithStock(10);

        item.Reserve(OrderId, 4);
        item.Reserve(OrderId, 4);

        Assert.Equal(4, item.QuantityReserved);
        Assert.Single(item.Reservations);
    }

    [Fact]
    public void Release_ReturnsReservedStock()
    {
        StockItem item = ItemWithStock(10);
        item.Reserve(OrderId, 4);

        item.Release(OrderId);

        Assert.Equal(0, item.QuantityReserved);
        Assert.Equal(10, item.QuantityAvailable);
        Assert.Equal(ReservationStatus.Released, Assert.Single(item.Reservations).Status);
    }

    [Fact]
    public void Commit_DeductsOnHandAndClearsReservation()
    {
        StockItem item = ItemWithStock(10);
        item.Reserve(OrderId, 4);

        item.Commit(OrderId);

        Assert.Equal(6, item.QuantityOnHand);
        Assert.Equal(0, item.QuantityReserved);
        Assert.Equal(6, item.QuantityAvailable);
    }

    [Fact]
    public void Commit_AfterRelease_DoesNothing()
    {
        StockItem item = ItemWithStock(10);
        item.Reserve(OrderId, 4);
        item.Release(OrderId);

        item.Commit(OrderId);

        Assert.Equal(10, item.QuantityOnHand);
    }

    [Fact]
    public void CanReserve_ChecksAvailableQuantity()
    {
        StockItem item = ItemWithStock(5);

        Assert.True(item.CanReserve(5));
        Assert.False(item.CanReserve(6));
        Assert.False(item.CanReserve(0));
    }

    private static StockItem NewItem() => StockItem.Create(Guid.NewGuid(), "SKU-1", "Espresso");

    private static StockItem ItemWithStock(int quantity)
    {
        StockItem item = NewItem();
        item.Receive(quantity);
        return item;
    }
}
