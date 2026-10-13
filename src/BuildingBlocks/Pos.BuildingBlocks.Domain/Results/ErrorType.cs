namespace Pos.BuildingBlocks.Domain.Results;

/// <summary>Classifies an <see cref="Error"/> so the API layer can map it to a status code.</summary>
public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
}
