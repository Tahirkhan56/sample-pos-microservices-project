using Pos.BuildingBlocks.Domain.Primitives;
using Pos.BuildingBlocks.Domain.Results;
using Pos.BuildingBlocks.Domain.ValueObjects;
using Pos.Payments.Domain.Payments.Events;

namespace Pos.Payments.Domain.Payments;

/// <summary>
/// The payment for one order. Exactly one provider payment (Stripe PaymentIntent) is used per order,
/// so retries after a decline re-confirm the same intent and the customer can never be charged twice.
/// </summary>
public sealed class Payment : AggregateRoot<Guid>
{
    public const int OrderNumberMaxLength = 32;
    public const int ProviderPaymentIdMaxLength = 128;
    public const int FailureReasonMaxLength = 500;

    private Payment(Guid id, Guid orderId, string orderNumber, Money amount, DateTime createdOnUtc)
        : base(id)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        Amount = amount;
        CreatedOnUtc = createdOnUtc;
        Status = PaymentStatus.Pending;
    }

    private Payment()
    {
    }

    public Guid OrderId { get; private set; }

    public string OrderNumber { get; private set; } = string.Empty;

    public Money Amount { get; private set; } = null!;

    public PaymentStatus Status { get; private set; }

    public string? ProviderPaymentId { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public DateTime? CompletedOnUtc { get; private set; }

    public int Version { get; private set; }

    public static Payment Create(Guid orderId, string orderNumber, Money amount, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(amount);
        return new Payment(Guid.CreateVersion7(), orderId, orderNumber, amount, utcNow);
    }

    public Result EnsureCanAcceptPayment() => Status switch
    {
        PaymentStatus.Succeeded => PaymentErrors.AlreadyPaid(OrderId),
        PaymentStatus.Cancelled => PaymentErrors.Cancelled(OrderId),
        _ => Result.Success(),
    };

    public Result AttachProviderPayment(string providerPaymentId)
    {
        Result canAccept = EnsureCanAcceptPayment();
        if (canAccept.IsFailure)
        {
            return canAccept;
        }

        if (ProviderPaymentId is not null && ProviderPaymentId != providerPaymentId)
        {
            return PaymentErrors.ProviderPaymentMismatch;
        }

        ProviderPaymentId = providerPaymentId;
        Status = PaymentStatus.Processing;
        FailureReason = null;
        Version++;
        return Result.Success();
    }

    public Result MarkSucceeded(DateTime utcNow)
    {
        if (Status == PaymentStatus.Succeeded)
        {
            return Result.Success();
        }

        if (Status == PaymentStatus.Cancelled)
        {
            return PaymentErrors.Cancelled(OrderId);
        }

        Status = PaymentStatus.Succeeded;
        FailureReason = null;
        CompletedOnUtc = utcNow;
        Version++;
        Raise(new PaymentSucceededDomainEvent(Id, OrderId, Amount));
        return Result.Success();
    }

    public Result MarkFailed(string reason)
    {
        if (Status is PaymentStatus.Succeeded or PaymentStatus.Cancelled)
        {
            return PaymentErrors.InvalidTransition(Status, PaymentStatus.Failed);
        }

        Status = PaymentStatus.Failed;
        FailureReason = reason.Length > FailureReasonMaxLength ? reason[..FailureReasonMaxLength] : reason;
        Version++;
        Raise(new PaymentFailedDomainEvent(Id, OrderId, FailureReason));
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == PaymentStatus.Cancelled)
        {
            return Result.Success();
        }

        if (Status == PaymentStatus.Succeeded)
        {
            return PaymentErrors.InvalidTransition(Status, PaymentStatus.Cancelled);
        }

        Status = PaymentStatus.Cancelled;
        Version++;
        return Result.Success();
    }
}
