using Pos.BuildingBlocks.Domain.Results;
using Pos.Payments.Application.Abstractions;

namespace Pos.Payments.UnitTests.Fakes;

internal sealed class FakePaymentGateway : IPaymentGateway
{
    public ProviderPaymentStatus NextStatus { get; set; } = ProviderPaymentStatus.RequiresPaymentMethod;

    public string? NextFailureReason { get; set; }

    public List<string> Calls { get; } = [];

    public Task<Result<ProviderPaymentResult>> CreatePaymentAsync(
        ProviderPaymentRequest request,
        string? paymentMethodId,
        CancellationToken cancellationToken)
    {
        Calls.Add(nameof(CreatePaymentAsync));
        return Task.FromResult(Respond($"pi_{request.PaymentId:N}"));
    }

    public Task<Result<ProviderPaymentResult>> GetPaymentAsync(string providerPaymentId, CancellationToken cancellationToken)
    {
        Calls.Add(nameof(GetPaymentAsync));
        return Task.FromResult(Respond(providerPaymentId));
    }

    public Task<Result<ProviderPaymentResult>> ConfirmPaymentAsync(
        string providerPaymentId,
        string paymentMethodId,
        CancellationToken cancellationToken)
    {
        Calls.Add(nameof(ConfirmPaymentAsync));
        return Task.FromResult(Respond(providerPaymentId));
    }

    public Task<Result> CancelPaymentAsync(string providerPaymentId, CancellationToken cancellationToken)
    {
        Calls.Add(nameof(CancelPaymentAsync));
        return Task.FromResult(Result.Success());
    }

    public Result<PaymentWebhookEvent> ParseWebhook(string payload, string signatureHeader) => PaymentWebhookEvent.Ignored;

    private Result<ProviderPaymentResult> Respond(string id) =>
        new ProviderPaymentResult(id, $"{id}_secret", NextStatus, NextFailureReason);
}
