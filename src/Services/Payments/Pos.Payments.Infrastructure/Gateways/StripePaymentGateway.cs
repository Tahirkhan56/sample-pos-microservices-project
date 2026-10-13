using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pos.BuildingBlocks.Domain.Results;
using Pos.Payments.Application.Abstractions;
using Stripe;

namespace Pos.Payments.Infrastructure.Gateways;

/// <summary>Stripe adapter for <see cref="IPaymentGateway"/> using the PaymentIntents API.</summary>
internal sealed partial class StripePaymentGateway(
    IStripeClient stripeClient,
    IOptions<StripeOptions> options,
    ILogger<StripePaymentGateway> logger) : IPaymentGateway
{
    private const string PaymentIntentSucceeded = "payment_intent.succeeded";
    private const string PaymentIntentPaymentFailed = "payment_intent.payment_failed";

    private readonly PaymentIntentService _paymentIntents = new(stripeClient);

    public async Task<Result<ProviderPaymentResult>> CreatePaymentAsync(
        ProviderPaymentRequest request,
        string? paymentMethodId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var createOptions = new PaymentIntentCreateOptions
        {
            Amount = StripeAmountConverter.ToMinorUnits(request.Amount),
            Currency = StripeAmountConverter.ToStripeCurrency(request.Amount.Currency),
            Description = $"POS order {request.OrderNumber}",
            Metadata = new Dictionary<string, string>
            {
                ["payment_id"] = request.PaymentId.ToString(),
                ["order_id"] = request.OrderId.ToString(),
                ["order_number"] = request.OrderNumber,
            },
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
                AllowRedirects = "never", // In-store flow: no redirect-based methods.
            },
            PaymentMethod = paymentMethodId,
            Confirm = paymentMethodId is not null,
        };

        // Idempotency key = our payment id: a retried request can never create a second PaymentIntent.
        var requestOptions = new RequestOptions { IdempotencyKey = $"pos-payment-{request.PaymentId}" };

        return await ExecuteAsync(() => _paymentIntents.CreateAsync(createOptions, requestOptions, cancellationToken));
    }

    public Task<Result<ProviderPaymentResult>> GetPaymentAsync(string providerPaymentId, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _paymentIntents.GetAsync(providerPaymentId, options: null, requestOptions: null, cancellationToken));

    public Task<Result<ProviderPaymentResult>> ConfirmPaymentAsync(
        string providerPaymentId,
        string paymentMethodId,
        CancellationToken cancellationToken)
    {
        var confirmOptions = new PaymentIntentConfirmOptions { PaymentMethod = paymentMethodId };
        return ExecuteAsync(() => _paymentIntents.ConfirmAsync(providerPaymentId, confirmOptions, requestOptions: null, cancellationToken));
    }

    public async Task<Result> CancelPaymentAsync(string providerPaymentId, CancellationToken cancellationToken)
    {
        Result<ProviderPaymentResult> result =
            await ExecuteAsync(() => _paymentIntents.CancelAsync(providerPaymentId, options: null, requestOptions: null, cancellationToken));
        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
    }

    public Result<PaymentWebhookEvent> ParseWebhook(string payload, string signatureHeader)
    {
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                payload,
                signatureHeader,
                options.Value.WebhookSecret,
                throwOnApiVersionMismatch: false);
        }
        catch (StripeException exception)
        {
            LogInvalidWebhook(logger, exception);
            return Error.Validation("Payment.InvalidWebhook", "The webhook signature is invalid.");
        }

        if (stripeEvent.Data.Object is not PaymentIntent intent)
        {
            return PaymentWebhookEvent.Ignored;
        }

        return stripeEvent.Type switch
        {
            PaymentIntentSucceeded => new PaymentWebhookEvent(PaymentWebhookEventKind.Succeeded, intent.Id, null),
            PaymentIntentPaymentFailed => new PaymentWebhookEvent(
                PaymentWebhookEventKind.Failed,
                intent.Id,
                intent.LastPaymentError?.Message ?? "Card payment failed."),
            _ => PaymentWebhookEvent.Ignored,
        };
    }

    private async Task<Result<ProviderPaymentResult>> ExecuteAsync(Func<Task<PaymentIntent>> call)
    {
        try
        {
            PaymentIntent intent = await call();
            return Map(intent);
        }
        catch (StripeException exception) when (exception.StripeError?.Type == "card_error" && exception.StripeError.PaymentIntent is not null)
        {
            // Declines are a business outcome, not an infrastructure failure.
            PaymentIntent declined = exception.StripeError.PaymentIntent;
            return new ProviderPaymentResult(declined.Id, declined.ClientSecret, ProviderPaymentStatus.Failed, exception.StripeError.Message);
        }
        catch (StripeException exception)
        {
            LogStripeError(logger, exception, exception.StripeError?.Type, exception.StripeError?.Code);
            return Error.Failure("Payment.Gateway", exception.StripeError?.Message ?? "The payment provider rejected the request.");
        }
    }

    private static ProviderPaymentResult Map(PaymentIntent intent) => new(
        intent.Id,
        intent.ClientSecret,
        MapStatus(intent.Status),
        intent.LastPaymentError?.Message);

    private static ProviderPaymentStatus MapStatus(string status) => status switch
    {
        "requires_payment_method" => ProviderPaymentStatus.RequiresPaymentMethod,
        "requires_confirmation" => ProviderPaymentStatus.RequiresConfirmation,
        "requires_action" => ProviderPaymentStatus.RequiresAction,
        "processing" => ProviderPaymentStatus.Processing,
        "succeeded" => ProviderPaymentStatus.Succeeded,
        "canceled" => ProviderPaymentStatus.Canceled,
        _ => ProviderPaymentStatus.Processing,
    };

    [LoggerMessage(Level = LogLevel.Warning, Message = "Rejected Stripe webhook with invalid signature")]
    private static partial void LogInvalidWebhook(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Stripe request failed: {ErrorType} {ErrorCode}")]
    private static partial void LogStripeError(ILogger logger, Exception exception, string? errorType, string? errorCode);
}
