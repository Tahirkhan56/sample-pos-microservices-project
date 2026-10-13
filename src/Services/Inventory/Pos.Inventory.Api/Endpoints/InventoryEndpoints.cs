using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Application.Pagination;
using Pos.BuildingBlocks.Domain.Results;
using Pos.BuildingBlocks.Web.Endpoints;
using Pos.BuildingBlocks.Web.Errors;
using Pos.Inventory.Application.Stock;

namespace Pos.Inventory.Api.Endpoints;

internal sealed class InventoryEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v1/inventory").WithTags("Inventory");

        group.MapGet("/", GetStockItemsAsync)
            .WithName("GetStockItems")
            .WithSummary("Lists stock levels.");

        group.MapGet("/{productId:guid}", GetStockAsync)
            .WithName("GetStockByProductId")
            .WithSummary("Gets the stock level for one product.");

        group.MapPost("/{productId:guid}/receive", ReceiveStockAsync)
            .WithName("ReceiveStock")
            .WithSummary("Adds received goods to on-hand quantity.");
    }

    private static async Task<IResult> GetStockItemsAsync(
        int? page,
        int? pageSize,
        IQueryHandler<GetStockItemsQuery, PagedResult<StockItemResponse>> handler,
        CancellationToken cancellationToken)
    {
        Result<PagedResult<StockItemResponse>> result =
            await handler.HandleAsync(new GetStockItemsQuery(page ?? 1, pageSize ?? 20), cancellationToken);
        return result.Match(value => Results.Ok(value));
    }

    private static async Task<IResult> GetStockAsync(
        Guid productId,
        IQueryHandler<GetStockByProductIdQuery, StockItemResponse> handler,
        CancellationToken cancellationToken)
    {
        Result<StockItemResponse> result = await handler.HandleAsync(new GetStockByProductIdQuery(productId), cancellationToken);
        return result.Match(value => Results.Ok(value));
    }

    private static async Task<IResult> ReceiveStockAsync(
        Guid productId,
        ReceiveStockRequest request,
        ICommandHandler<ReceiveStockCommand, StockItemResponse> handler,
        CancellationToken cancellationToken)
    {
        Result<StockItemResponse> result = await handler.HandleAsync(new ReceiveStockCommand(productId, request.Quantity), cancellationToken);
        return result.Match(value => Results.Ok(value));
    }
}

internal sealed record ReceiveStockRequest(int Quantity);
