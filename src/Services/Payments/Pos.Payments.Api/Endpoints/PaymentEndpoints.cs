using Microsoft.Extensions.Options;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Domain.Results;
using Pos.BuildingBlocks.Web.Endpoints;
using Pos.BuildingBlocks.Web.Errors;
using Pos.Payments.Application.Payments;
using Pos.Payments.Application.Payments.CreatePaymentIntent;
using Pos.Payments.Application.Payments.GetPayment;
using Pos.Payments.Application.Payments.HandleWebhook;
using Pos.Payments.Infrastructure.Gateways;

namespace Pos.Payments.Api.Endpoints;

internal sealed class PaymentEndpoints : IEndpoint
{
    private const string StripeSignatureHeader = "Stripe-Signature";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v1/payments").WithTags("Payments");

        group.MapGet("/config", GetConfig)
            .WithName("GetPaymentConfig")
            .WithSummary("Returns the Stripe publishable key for the till UI.");

        group.MapGet("/orders/{orderId:guid}", GetPaymentAsync)
            .WithName("GetPaymentByOrderId");

        group.MapPost("/orders/{orderId:guid}/intent", CreatePaymentIntentAsync)
            .WithName("CreatePaymentIntent")
            .WithSummary("Creates or resumes the Stripe PaymentIntent for an order (optionally confirming it).");

        group.MapPost("/webhooks/stripe", HandleStripeWebhookAsync)
            .WithName("StripeWebhook")
            .WithSummary("Stripe webhook receiver (signature verified).")
            .ExcludeFromDescription();
    }

    private static IResult GetConfig(IOptions<StripeOptions> options) =>
        Results.Ok(new PaymentConfigResponse(options.Value.PublishableKey));

    private static async Task<IResult> GetPaymentAsync(
        Guid orderId,
        IQueryHandler<GetPaymentByOrderIdQuery, PaymentResponse> handler,
        CancellationToken cancellationToken)
    {
        Result<PaymentResponse> result = await handler.HandleAsync(new GetPaymentByOrderIdQuery(orderId), cancellationToken);
        return result.Match(value => Results.Ok(value));
    }

    private static async Task<IResult> CreatePaymentIntentAsync(
        Guid orderId,
        CreatePaymentIntentRequest? request,
        ICommandHandler<CreatePaymentIntentCommand, PaymentIntentResponse> handler,
        CancellationToken cancellationToken)
    {
        Result<PaymentIntentResponse> result =
            await handler.HandleAsync(new CreatePaymentIntentCommand(orderId, request?.PaymentMethodId), cancellationToken);
        return result.Match(value => Results.Ok(value));
    }

    private static async Task<IResult> HandleStripeWebhookAsync(
        HttpRequest request,
        ICommandHandler<HandlePaymentWebhookCommand> handler,
        CancellationToken cancellationToken)
    {
        // Signature verification needs the exact raw body, so read it ourselves instead of model binding.
        using var reader = new StreamReader(request.Body);
        string payload = await reader.ReadToEndAsync(cancellationToken);
        string signature = request.Headers[StripeSignatureHeader].ToString();

        Result result = await handler.HandleAsync(new HandlePaymentWebhookCommand(payload, signature), cancellationToken);
        return result.Match(() => Results.Ok());
    }
}

internal sealed record CreatePaymentIntentRequest(string? PaymentMethodId);

internal sealed record PaymentConfigResponse(string PublishableKey);
