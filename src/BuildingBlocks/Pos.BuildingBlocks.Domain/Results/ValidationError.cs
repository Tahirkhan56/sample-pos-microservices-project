namespace Pos.BuildingBlocks.Domain.Results;

/// <summary>Aggregates several validation failures into a single error.</summary>
public sealed record ValidationError(IReadOnlyCollection<Error> Errors)
    : Error("Validation.General", "One or more validation errors occurred.", ErrorType.Validation);
