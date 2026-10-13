using Pos.BuildingBlocks.Application.Messaging;

namespace Pos.Ordering.Application.Orders.PlaceOrder;

public sealed record PlaceOrderCommand(string CashierId, IReadOnlyList<PlaceOrderItem> Items) : ICommand<OrderResponse>;

public sealed record PlaceOrderItem(Guid ProductId, int Quantity);
