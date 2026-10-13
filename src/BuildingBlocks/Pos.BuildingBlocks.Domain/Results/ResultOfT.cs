namespace Pos.BuildingBlocks.Domain.Results;

/// <summary>Outcome of an operation that returns a value on success.</summary>
/// <typeparam name="TValue">The value type.</typeparam>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>The value. Accessing it on a failed result is a programming error.</summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failed result cannot be accessed.");

#pragma warning disable CA2225 // Operator overloads have named alternates: Result.Success / Result.Failure.
    public static implicit operator Result<TValue>(TValue value) => Success(value);

    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);
#pragma warning restore CA2225
}
