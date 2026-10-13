using FluentValidation;
using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Domain.Results;
using Pos.Inventory.Domain.StockItems;

namespace Pos.Inventory.Application.Stock;

/// <summary>Goods received from a supplier.</summary>
public sealed record ReceiveStockCommand(Guid ProductId, int Quantity) : ICommand<StockItemResponse>;

internal sealed class ReceiveStockCommandValidator : AbstractValidator<ReceiveStockCommand>
{
    public ReceiveStockCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.Quantity).GreaterThan(0).LessThanOrEqualTo(100_000);
    }
}

internal sealed class ReceiveStockCommandHandler(IStockItemRepository stockItems, IUnitOfWork unitOfWork)
    : ICommandHandler<ReceiveStockCommand, StockItemResponse>
{
    public async Task<Result<StockItemResponse>> HandleAsync(ReceiveStockCommand command, CancellationToken cancellationToken)
    {
        StockItem? item = await stockItems.GetByProductIdAsync(command.ProductId, cancellationToken);
        if (item is null)
        {
            return StockItemErrors.NotFound(command.ProductId);
        }

        Result result = item.Receive(command.Quantity);
        if (result.IsFailure)
        {
            return result.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return item.ToResponse();
    }
}
