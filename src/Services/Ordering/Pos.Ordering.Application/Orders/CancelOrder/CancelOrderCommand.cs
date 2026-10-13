using FluentValidation;
using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Domain.Results;
using Pos.Ordering.Domain.Orders;

namespace Pos.Ordering.Application.Orders.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId, string Reason) : ICommand;

internal sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(Order.ReasonMaxLength);
    }
}

internal sealed class CancelOrderCommandHandler(IOrderRepository orders, IUnitOfWork unitOfWork)
    : ICommandHandler<CancelOrderCommand>
{
    public async Task<Result> HandleAsync(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        Order? order = await orders.GetByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
        {
            return OrderErrors.NotFound(command.OrderId);
        }

        Result result = order.Cancel(command.Reason);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
