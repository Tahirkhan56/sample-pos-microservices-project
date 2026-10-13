namespace Pos.BuildingBlocks.Application.Abstractions;

/// <summary>Commits all changes tracked in the current scope atomically.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
