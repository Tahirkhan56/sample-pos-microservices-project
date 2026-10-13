namespace Pos.BuildingBlocks.Application.Abstractions;

/// <summary>Distributed cache abstraction (backed by Redis in production).</summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken);

    Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration, CancellationToken cancellationToken);

    Task RemoveAsync(string key, CancellationToken cancellationToken);
}
