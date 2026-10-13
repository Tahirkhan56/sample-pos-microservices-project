using Pos.BuildingBlocks.Domain.ValueObjects;
using Pos.Payments.Application.Abstractions;
using Pos.Payments.Application.Payments.CreatePaymentIntent;
using Pos.Payments.Domain.Payments;
using Pos.Payments.Domain.Payments.Events;
using Pos.Payments.UnitTests.Fakes;

namespace Pos.Payments.UnitTests.Application;

public sealed class CreatePaymentIntentCommandHandlerTests
{
    private readonly FakePaymentGateway _gateway = new();
    private readonly InMemoryPaymentRepository _payments = new();
    private readonly CreatePaymentIntentCommandHandler _handler;
    private readonly Payment _payment;

    public CreatePaymentIntentCommandHandlerTests()
    {
        _payment = Payment.Create(Guid.NewGuid(), "POS-1", Money.Create(7.50m, "USD").Value, DateTime.UtcNow);
        _payments.Add(_payment);
        _handler = new CreatePaymentIntentCommandHandler(_payments, _gateway, _payments, TimeProvider.System);
    }

    [Fact]
    public async Task HandleAsync_UnknownOrder_ReturnsNotFound()
    {
        var result = await _handler.HandleAsync(new CreatePaymentIntentCommand(Guid.NewGuid(), null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Payment.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_FirstCall_CreatesIntentAndReturnsClientSecret()
    {
        var result = await _handler.HandleAsync(new CreatePaymentIntentCommand(_payment.OrderId, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.ClientSecret);
        Assert.Equal(PaymentStatus.Processing, _payment.Status);
        Assert.Equal(new[] { nameof(IPaymentGateway.CreatePaymentAsync) }, _gateway.Calls);
    }

    [Fact]
    public async Task HandleAsync_SecondCall_ReusesExistingIntent()
    {
        await _handler.HandleAsync(new CreatePaymentIntentCommand(_payment.OrderId, null), CancellationToken.None);
        await _handler.HandleAsync(new CreatePaymentIntentCommand(_payment.OrderId, "pm_card_visa"), CancellationToken.None);

        Assert.Equal(
            new[] { nameof(IPaymentGateway.CreatePaymentAsync), nameof(IPaymentGateway.ConfirmPaymentAsync) },
            _gateway.Calls);
    }

    [Fact]
    public async Task HandleAsync_ImmediateSuccess_MarksPaymentSucceeded()
    {
        _gateway.NextStatus = ProviderPaymentStatus.Succeeded;

        var result = await _handler.HandleAsync(new CreatePaymentIntentCommand(_payment.OrderId, "pm_card_visa"), CancellationToken.None);

        Assert.Equal(PaymentStatus.Succeeded, result.Value.Status);
        Assert.Single(_payment.DomainEvents.OfType<PaymentSucceededDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_CardDeclined_MarksPaymentFailed()
    {
        _gateway.NextStatus = ProviderPaymentStatus.Failed;
        _gateway.NextFailureReason = "Your card was declined.";

        var result = await _handler.HandleAsync(
            new CreatePaymentIntentCommand(_payment.OrderId, "pm_card_chargeDeclined"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Failed, result.Value.Status);
        Assert.Equal("Your card was declined.", result.Value.FailureReason);
    }

    [Fact]
    public async Task HandleAsync_AlreadyPaid_ReturnsConflict()
    {
        _payment.MarkSucceeded(DateTime.UtcNow);

        var result = await _handler.HandleAsync(new CreatePaymentIntentCommand(_payment.OrderId, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Payment.AlreadyPaid", result.Error.Code);
        Assert.Empty(_gateway.Calls);
    }
}
