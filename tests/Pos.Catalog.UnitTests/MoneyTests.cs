using Pos.BuildingBlocks.Domain.ValueObjects;

namespace Pos.Catalog.UnitTests;

public sealed class MoneyTests
{
    [Fact]
    public void Create_WithNegativeAmount_ReturnsFailure()
    {
        var result = Money.Create(-1m, "USD");

        Assert.True(result.IsFailure);
        Assert.Equal(MoneyErrors.NegativeAmount, result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("US1")]
    public void Create_WithInvalidCurrency_ReturnsFailure(string currency)
    {
        var result = Money.Create(10m, currency);

        Assert.True(result.IsFailure);
        Assert.Equal(MoneyErrors.InvalidCurrency, result.Error);
    }

    [Fact]
    public void Create_NormalizesCurrencyAndRoundsToCents()
    {
        Money money = Money.Create(10.005m, "usd").Value;

        Assert.Equal("USD", money.Currency);
        Assert.Equal(10.01m, money.Amount);
    }

    [Fact]
    public void Add_WithSameCurrency_SumsAmounts()
    {
        Money total = Money.Create(1.25m, "USD").Value + Money.Create(2.50m, "USD").Value;

        Assert.Equal(3.75m, total.Amount);
    }

    [Fact]
    public void Add_WithDifferentCurrencies_Throws()
    {
        Money usd = Money.Create(1m, "USD").Value;
        Money eur = Money.Create(1m, "EUR").Value;

        Assert.Throws<InvalidOperationException>(() => usd + eur);
    }

    [Fact]
    public void Multiply_ScalesAmount()
    {
        Money lineTotal = Money.Create(3.75m, "USD").Value.Multiply(3);

        Assert.Equal(11.25m, lineTotal.Amount);
    }

    [Fact]
    public void Equality_IsByValue()
    {
        Assert.Equal(Money.Create(5m, "USD").Value, Money.Create(5m, "usd").Value);
    }
}
