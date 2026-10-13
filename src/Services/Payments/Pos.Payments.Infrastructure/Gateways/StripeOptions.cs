using System.ComponentModel.DataAnnotations;

namespace Pos.Payments.Infrastructure.Gateways;

/// <summary>Stripe credentials. Supply via user-secrets, environment variables or a Kubernetes Secret — never commit them.</summary>
public sealed class StripeOptions
{
    public const string SectionName = "Stripe";

    /// <summary>Server-side secret key (<c>sk_test_...</c> / <c>sk_live_...</c>).</summary>
    [Required]
    public string SecretKey { get; init; } = string.Empty;

    /// <summary>Client-side publishable key (<c>pk_test_...</c>), returned to the till UI.</summary>
    [Required]
    public string PublishableKey { get; init; } = string.Empty;

    /// <summary>Signing secret of the webhook endpoint (<c>whsec_...</c>).</summary>
    [Required]
    public string WebhookSecret { get; init; } = string.Empty;
}
