using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Application.Pagination;
using Pos.BuildingBlocks.Domain.Results;
using Pos.BuildingBlocks.Web.Endpoints;
using Pos.BuildingBlocks.Web.Errors;
using Pos.Ordering.Application.Orders;
using Pos.Ordering.Application.Orders.CancelOrder;
using Pos.Ordering.Application.Orders.GetOrders;
using Pos.Ordering.Application.Orders.PlaceOrder;
using Pos.Ordering.Domain.Orders;

namespace Pos.Ordering.Api.Endpoints;

internal sealed class OrderEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v1/orders").WithTags("Orders");

        group.MapPost("/", PlaceOrderAsync)
            .WithName("PlaceOrder")
            .WithSummary("Rings up a sale. Returns 202: stock reservation continues asynchronously.");

        group.MapGet("/{id:guid}", GetOrderAsync)
            .WithName("GetOrderById");

        group.MapGet("/", GetOrdersAsync)
            .WithName("GetOrders")
            .WithSummary("Lists orders, newest first, optionally filtered by status.");

        group.MapPost("/{id:guid}/cancel", CancelOrderAsync)
            .WithName("CancelOrder")
            .WithSummary("Cancels an unpaid order and releases its stock.");
    }

    private static async Task<IResult> PlaceOrderAsync(
        PlaceOrderRequest request,
        ICommandHandler<PlaceOrderCommand, OrderResponse> handler,
        CancellationToken cancellationToken)
    {
        var command = new PlaceOrderCommand(
            request.CashierId,
            [.. request.Items.Select(item => new PlaceOrderItem(item.ProductId, item.Quantity))]);

        Result<OrderResponse> result = await handler.HandleAsync(command, cancellationToken);
        return result.Match(order => Results.AcceptedAtRoute("GetOrderById", new { id = order.Id }, order));
    }

    private static async Task<IResult> GetOrderAsync(
        Guid id,
        IQueryHandler<GetOrderByIdQuery, OrderResponse> handler,
        CancellationToken cancellationToken)
    {
        Result<OrderResponse> result = await handler.HandleAsync(new GetOrderByIdQuery(id), cancellationToken);
        return result.Match(value => Results.Ok(value));
    }

    private static async Task<IResult> GetOrdersAsync(
        OrderStatus? status,
        int? page,
        int? pageSize,
        IQueryHandler<GetOrdersQuery, PagedResult<OrderResponse>> handler,
        CancellationToken cancellationToken)
    {
        Result<PagedResult<OrderResponse>> result =
            await handler.HandleAsync(new GetOrdersQuery(status, page ?? 1, pageSize ?? 20), cancellationToken);
        return result.Match(value => Results.Ok(value));
    }

    private static async Task<IResult> CancelOrderAsync(
        Guid id,
        CancelOrderRequest request,
        ICommandHandler<CancelOrderCommand> handler,
        CancellationToken cancellationToken)
    {
        Result result = await handler.HandleAsync(new CancelOrderCommand(id, request.Reason), cancellationToken);
        return result.Match(Results.NoContent);
    }
}

internal sealed record PlaceOrderRequest(string CashierId, IReadOnlyList<PlaceOrderItemRequest> Items);

internal sealed record PlaceOrderItemRequest(Guid ProductId, int Quantity);

internal sealed record CancelOrderRequest(string Reason);
