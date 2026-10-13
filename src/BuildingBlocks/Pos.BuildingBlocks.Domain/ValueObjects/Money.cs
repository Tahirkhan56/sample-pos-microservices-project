using Pos.BuildingBlocks.Domain.Results;

namespace Pos.BuildingBlocks.Domain.ValueObjects;

/// <summary>An amount in a specific ISO 4217 currency. Immutable value object.</summary>
public sealed record Money
{
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>Required by EF Core complex-type materialization.</summary>
    private Money()
    {
        Currency = string.Empty;
    }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; }

    public static Result<Money> Create(decimal amount, string currency)
    {
        if (amount < 0)
        {
            return MoneyErrors.NegativeAmount;
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3 || !currency.All(char.IsAsciiLetter))
        {
            return MoneyErrors.InvalidCurrency;
        }

        return new Money(decimal.Round(amount, 2, MidpointRounding.AwayFromZero), currency.ToUpperInvariant());
    }

    public static Money Zero(string currency) => new(0m, currency.ToUpperInvariant());

    public static Money operator +(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        EnsureSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money Add(Money left, Money right) => left + right;

    public Money Multiply(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);
        return new Money(Amount * quantity, Currency);
    }

    public override string ToString() => $"{Amount:0.00} {Currency}";

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (!string.Equals(left.Currency, right.Currency, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Cannot combine {left.Currency} with {right.Currency}.");
        }
    }
}
