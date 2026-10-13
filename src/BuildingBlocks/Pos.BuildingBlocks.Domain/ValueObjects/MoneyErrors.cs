using Pos.BuildingBlocks.Domain.Results;

namespace Pos.BuildingBlocks.Domain.ValueObjects;

public static class MoneyErrors
{
    public static readonly Error NegativeAmount =
        Error.Validation("Money.NegativeAmount", "Amount cannot be negative.");

    public static readonly Error InvalidCurrency =
        Error.Validation("Money.InvalidCurrency", "Currency must be a three-letter ISO 4217 code.");
}
