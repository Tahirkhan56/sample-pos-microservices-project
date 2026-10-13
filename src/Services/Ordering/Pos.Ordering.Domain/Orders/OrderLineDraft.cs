using Pos.BuildingBlocks.Domain.ValueObjects;

namespace Pos.Ordering.Domain.Orders;

/// <summary>Input for a line when placing an order; the price is a snapshot taken from the catalog.</summary>
public sealed record OrderLineDraft(Guid ProductId, string ProductName, Money UnitPrice, int Quantity);
