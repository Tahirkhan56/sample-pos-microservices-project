namespace Pos.Contracts.Ordering;

public sealed record OrderPlacedIntegrationEvent(Guid OrderId, IReadOnlyList<OrderItemContract> Items) : IntegrationEvent;

public sealed record OrderItemContract(Guid ProductId, int Quantity);
