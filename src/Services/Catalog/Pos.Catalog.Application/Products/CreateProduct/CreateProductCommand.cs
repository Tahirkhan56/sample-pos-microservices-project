using Pos.BuildingBlocks.Application.Messaging;

namespace Pos.Catalog.Application.Products.CreateProduct;

public sealed record CreateProductCommand(
    string Sku,
    string Name,
    string? Description,
    string Category,
    decimal Price,
    string Currency) : ICommand<Guid>;
