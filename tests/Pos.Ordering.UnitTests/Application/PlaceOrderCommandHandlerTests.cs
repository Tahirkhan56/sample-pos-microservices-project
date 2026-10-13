using Pos.BuildingBlocks.Domain.Results;
using Pos.Ordering.Application.Abstractions;
using Pos.Ordering.Application.Orders.PlaceOrder;
using Pos.Ordering.Domain.Orders;
using Pos.Ordering.UnitTests.Fakes;

namespace Pos.Ordering.UnitTests.Application;

public sealed class PlaceOrderCommandHandlerTests
{
    private static readonly Guid Latte = Guid.NewGuid();

    private readonly FakeCatalogService _catalog = new();
    private readonly InMemoryOrderRepository _orders = new();
    private readonly PlaceOrderCommandHandler _handler;

    public PlaceOrderCommandHandlerTests()
    {
        _catalog.Products.Add(new CatalogProduct(Latte, "COF-LAT-001", "Caffe Latte", 3.75m, "USD", IsActive: true));
        _handler = new PlaceOrderCommandHandler(_catalog, _orders, _orders, new FixedTimeProvider(DateTimeOffset.UtcNow));
    }

    [Fact]
    public async Task HandleAsync_WithKnownProducts_PersistsPendingOrderWithCatalogPrices()
    {
        var result = await _handler.HandleAsync(new PlaceOrderCommand("cashier-7", [new PlaceOrderItem(Latte, 2)]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(7.50m, result.Value.Total);
        Assert.Equal(OrderStatus.Pending, result.Value.Status);
        Assert.Single(_orders.Orders);
        Assert.Equal(1, _orders.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownProduct_ReturnsValidationErrorAndSavesNothing()
    {
        var result = await _handler.HandleAsync(
            new PlaceOrderCommand("cashier-7", [new PlaceOrderItem(Guid.NewGuid(), 1)]),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Order.ProductUnavailable", result.Error.Code);
        Assert.Empty(_orders.Orders);
        Assert.Equal(0, _orders.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_WhenCatalogUnavailable_PropagatesError()
    {
        _catalog.FailWith = Error.Failure("Catalog.Unavailable", "down");

        var result = await _handler.HandleAsync(new PlaceOrderCommand("cashier-7", [new PlaceOrderItem(Latte, 1)]), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Catalog.Unavailable", result.Error.Code);
    }
}
