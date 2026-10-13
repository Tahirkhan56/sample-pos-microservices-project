using FluentValidation;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.Payments.Application.Abstractions;
using Pos.Payments.Domain.Payments;

namespace Pos.Payments.Application.Payments.CreatePaymentIntent;

/// <summary>
/// Starts (or resumes) card payment for an order. Returns the Stripe client secret for Stripe.js / Terminal.
/// Supplying <see cref="PaymentMethodId"/> (e.g. <c>pm_card_visa</c> in test mode) confirms server-side.
/// </summary>
public sealed record CreatePaymentIntentCommand(Guid OrderId, string? PaymentMethodId) : ICommand<PaymentIntentResponse>;

public sealed record PaymentIntentResponse(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    string ProviderPaymentId,
    string? ClientSecret,
    ProviderPaymentStatus ProviderStatus,
    string? FailureReason);

internal sealed class CreatePaymentIntentCommandValidator : AbstractValidator<CreatePaymentIntentCommand>
{
    public CreatePaymentIntentCommandValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
        RuleFor(command => command.PaymentMethodId).MaximumLength(255);
    }
}
