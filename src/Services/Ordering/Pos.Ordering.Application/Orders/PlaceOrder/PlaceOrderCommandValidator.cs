using FluentValidation;
using Pos.Ordering.Domain.Orders;

namespace Pos.Ordering.Application.Orders.PlaceOrder;

internal sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(command => command.CashierId).NotEmpty().MaximumLength(Order.CashierIdMaxLength);
        RuleFor(command => command.Items)
            .NotEmpty()
            .Must(items => items.Count <= Order.MaxLines)
            .WithMessage($"An order cannot have more than {Order.MaxLines} lines.");
        RuleForEach(command => command.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0).LessThanOrEqualTo(1_000);
        });
    }
}
