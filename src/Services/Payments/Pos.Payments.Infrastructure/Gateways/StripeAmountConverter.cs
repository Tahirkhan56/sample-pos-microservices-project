using System.Collections.Frozen;
using Pos.BuildingBlocks.Domain.ValueObjects;

namespace Pos.Payments.Infrastructure.Gateways;

/// <summary>
/// Stripe expects amounts in the currency's smallest unit (cents for USD, but whole yen for JPY).
/// See https://docs.stripe.com/currencies#zero-decimal.
/// </summary>
internal static class StripeAmountConverter
{
    private static readonly FrozenSet<string> ZeroDecimalCurrencies = new[]
    {
        "BIF", "CLP", "DJF", "GNF", "JPY", "KMF", "KRW", "MGA", "PYG", "RWF", "UGX", "VND", "VUV", "XAF", "XOF", "XPF",
    }.ToFrozenSet(StringComparer.Ordinal);

    public static long ToMinorUnits(Money money)
    {
        ArgumentNullException.ThrowIfNull(money);
        decimal factor = ZeroDecimalCurrencies.Contains(money.Currency) ? 1m : 100m;
        return (long)decimal.Round(money.Amount * factor, 0, MidpointRounding.AwayFromZero);
    }

#pragma warning disable CA1308 // Stripe requires lowercase ISO currency codes.
    public static string ToStripeCurrency(string currency) => currency.ToLowerInvariant();
#pragma warning restore CA1308
}
