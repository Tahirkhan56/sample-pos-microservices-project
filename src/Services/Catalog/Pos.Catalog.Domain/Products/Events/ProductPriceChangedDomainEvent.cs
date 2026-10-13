using Pos.BuildingBlocks.Domain.Primitives;
using Pos.BuildingBlocks.Domain.ValueObjects;

namespace Pos.Catalog.Domain.Products.Events;

public sealed record ProductPriceChangedDomainEvent(Guid ProductId, Money OldPrice, Money NewPrice) : DomainEvent;
