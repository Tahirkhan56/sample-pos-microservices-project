using Microsoft.AspNetCore.Mvc;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Application.Pagination;
using Pos.BuildingBlocks.Domain.Results;
using Pos.BuildingBlocks.Web.Endpoints;
using Pos.BuildingBlocks.Web.Errors;
using Pos.Catalog.Application.Products;
using Pos.Catalog.Application.Products.ChangeProductPrice;
using Pos.Catalog.Application.Products.CreateProduct;
using Pos.Catalog.Application.Products.DeactivateProduct;
using Pos.Catalog.Application.Products.GetProductById;
using Pos.Catalog.Application.Products.GetProducts;
using Pos.Catalog.Application.Products.GetProductsByIds;
using Pos.Catalog.Application.Products.UpdateProduct;

namespace Pos.Catalog.Api.Endpoints;

internal sealed class ProductEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v1/products").WithTags("Products");

        group.MapGet("/", GetProductsAsync)
            .WithName("GetProducts")
            .WithSummary("Lists active products with paging and optional search.");

        group.MapGet("/{id:guid}", GetProductByIdAsync)
            .WithName("GetProductById")
            .WithSummary("Gets a single product (served from Redis when cached).");

        group.MapGet("/lookup", LookupProductsAsync)
            .WithName("LookupProducts")
            .WithSummary("Batch lookup by ids, used by the Ordering service.");

        group.MapPost("/", CreateProductAsync)
            .WithName("CreateProduct")
            .WithSummary("Creates a product and notifies Inventory.");

        group.MapPut("/{id:guid}", UpdateProductAsync)
            .WithName("UpdateProduct");

        group.MapPut("/{id:guid}/price", ChangePriceAsync)
            .WithName("ChangeProductPrice");

        group.MapDelete("/{id:guid}", DeactivateProductAsync)
            .WithName("DeactivateProduct")
            .WithSummary("Soft-deletes (deactivates) a product.");
    }

    private static async Task<IResult> GetProductsAsync(
        [AsParameters] GetProductsRequest request,
        IQueryHandler<GetProductsQuery, PagedResult<ProductResponse>> handler,
        CancellationToken cancellationToken)
    {
        Result<PagedResult<ProductResponse>> result = await handler.HandleAsync(
            new GetProductsQuery(request.Search, request.Category, request.Page ?? 1, request.PageSize ?? 20),
            cancellationToken);
        return result.Match(value => Results.Ok(value));
    }

    private static async Task<IResult> GetProductByIdAsync(
        Guid id,
        IQueryHandler<GetProductByIdQuery, ProductResponse> handler,
        CancellationToken cancellationToken)
    {
        Result<ProductResponse> result = await handler.HandleAsync(new GetProductByIdQuery(id), cancellationToken);
        return result.Match(value => Results.Ok(value));
    }

    private static async Task<IResult> LookupProductsAsync(
        [FromQuery(Name = "ids")] Guid[] ids,
        IQueryHandler<GetProductsByIdsQuery, IReadOnlyList<ProductResponse>> handler,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<ProductResponse>> result = await handler.HandleAsync(new GetProductsByIdsQuery(ids), cancellationToken);
        return result.Match(value => Results.Ok(value));
    }

    private static async Task<IResult> CreateProductAsync(
        CreateProductRequest request,
        ICommandHandler<CreateProductCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.Sku,
            request.Name,
            request.Description,
            request.Category,
            request.Price,
            request.Currency);

        Result<Guid> result = await handler.HandleAsync(command, cancellationToken);
        return result.Match(id => Results.CreatedAtRoute("GetProductById", new { id }, new { id }));
    }

    private static async Task<IResult> UpdateProductAsync(
        Guid id,
        UpdateProductRequest request,
        ICommandHandler<UpdateProductCommand> handler,
        CancellationToken cancellationToken)
    {
        Result result = await handler.HandleAsync(
            new UpdateProductCommand(id, request.Name, request.Description, request.Category),
            cancellationToken);
        return result.Match(Results.NoContent);
    }

    private static async Task<IResult> ChangePriceAsync(
        Guid id,
        ChangePriceRequest request,
        ICommandHandler<ChangeProductPriceCommand> handler,
        CancellationToken cancellationToken)
    {
        Result result = await handler.HandleAsync(new ChangeProductPriceCommand(id, request.Price, request.Currency), cancellationToken);
        return result.Match(Results.NoContent);
    }

    private static async Task<IResult> DeactivateProductAsync(
        Guid id,
        ICommandHandler<DeactivateProductCommand> handler,
        CancellationToken cancellationToken)
    {
        Result result = await handler.HandleAsync(new DeactivateProductCommand(id), cancellationToken);
        return result.Match(Results.NoContent);
    }
}

internal sealed record GetProductsRequest(string? Search, string? Category, int? Page, int? PageSize);

internal sealed record CreateProductRequest(
    string Sku,
    string Name,
    string? Description,
    string Category,
    decimal Price,
    string Currency = "USD");

internal sealed record UpdateProductRequest(string Name, string? Description, string Category);

internal sealed record ChangePriceRequest(decimal Price, string Currency = "USD");
