using Pos.BuildingBlocks.Domain.ValueObjects;
using Pos.Payments.Infrastructure.Gateways;

namespace Pos.Payments.UnitTests.Infrastructure;

public sealed class StripeAmountConverterTests
{
    [Theory]
    [InlineData(12.34, "USD", 1234)]
    [InlineData(0.50, "EUR", 50)]
    [InlineData(1500, "JPY", 1500)]
    [InlineData(25000, "KRW", 25000)]
    public void ToMinorUnits_RespectsZeroDecimalCurrencies(double amount, string currency, long expected)
    {
        Money money = Money.Create((decimal)amount, currency).Value;

        Assert.Equal(expected, StripeAmountConverter.ToMinorUnits(money));
    }

    [Fact]
    public void ToStripeCurrency_IsLowercase()
    {
        Assert.Equal("usd", StripeAmountConverter.ToStripeCurrency("USD"));
    }
}
