using Pos.BuildingBlocks.Domain.ValueObjects;
using Pos.Payments.Domain.Payments;
using Pos.Payments.Domain.Payments.Events;

namespace Pos.Payments.UnitTests.Domain;

public sealed class PaymentTests
{
    private static readonly DateTime Now = new(2026, 10, 13, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_StartsPendingWithoutEvents()
    {
        Payment payment = NewPayment();

        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Empty(payment.DomainEvents);
    }

    [Fact]
    public void AttachProviderPayment_MovesToProcessing()
    {
        Payment payment = NewPayment();

        Assert.True(payment.AttachProviderPayment("pi_123").IsSuccess);

        Assert.Equal(PaymentStatus.Processing, payment.Status);
        Assert.Equal("pi_123", payment.ProviderPaymentId);
    }

    [Fact]
    public void AttachProviderPayment_WithDifferentId_ReturnsConflict()
    {
        Payment payment = NewPayment();
        payment.AttachProviderPayment("pi_123");

        var result = payment.AttachProviderPayment("pi_999");

        Assert.True(result.IsFailure);
        Assert.Equal(PaymentErrors.ProviderPaymentMismatch, result.Error);
    }

    [Fact]
    public void MarkSucceeded_IsIdempotentAndRaisesEventOnce()
    {
        Payment payment = NewPayment();
        payment.AttachProviderPayment("pi_123");

        payment.MarkSucceeded(Now);
        payment.MarkSucceeded(Now);

        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
        Assert.Equal(Now, payment.CompletedOnUtc);
        Assert.Single(payment.DomainEvents.OfType<PaymentSucceededDomainEvent>());
    }

    [Fact]
    public void MarkFailed_AfterSuccess_ReturnsInvalidTransition()
    {
        Payment payment = NewPayment();
        payment.MarkSucceeded(Now);

        Assert.True(payment.MarkFailed("late decline").IsFailure);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
    }

    [Fact]
    public void MarkFailed_ThenRetry_AllowsNewAttemptOnSameIntent()
    {
        Payment payment = NewPayment();
        payment.AttachProviderPayment("pi_123");
        payment.MarkFailed("Your card was declined.");

        Assert.True(payment.EnsureCanAcceptPayment().IsSuccess);
        Assert.True(payment.AttachProviderPayment("pi_123").IsSuccess);
        Assert.Null(payment.FailureReason);
    }

    [Fact]
    public void Cancel_AfterSuccess_ReturnsInvalidTransition()
    {
        Payment payment = NewPayment();
        payment.MarkSucceeded(Now);

        Assert.True(payment.Cancel().IsFailure);
    }

    [Fact]
    public void EnsureCanAcceptPayment_WhenCancelled_ReturnsConflict()
    {
        Payment payment = NewPayment();
        payment.Cancel();

        Assert.Equal("Payment.Cancelled", payment.EnsureCanAcceptPayment().Error.Code);
    }

    private static Payment NewPayment() =>
        Payment.Create(Guid.NewGuid(), "POS-20261013-ABCDEF12", Money.Create(12.30m, "USD").Value, Now);
}
