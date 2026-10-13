namespace Pos.BuildingBlocks.Domain.Results;

/// <summary>An expected, business-level failure. Exceptions are reserved for the unexpected.</summary>
/// <param name="Code">Stable, machine-readable code, e.g. <c>Order.NotFound</c>.</param>
/// <param name="Description">Human-readable explanation.</param>
/// <param name="Type">Category used for HTTP mapping.</param>
public record Error(string Code, string Description, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);

    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);

    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
}
