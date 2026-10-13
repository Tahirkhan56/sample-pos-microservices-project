using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Pos.BuildingBlocks.Application.Abstractions;

namespace Pos.BuildingBlocks.Infrastructure.Caching;

/// <summary>JSON-serializing wrapper around <see cref="IDistributedCache"/> (Redis).</summary>
internal sealed class DistributedCacheService(IDistributedCache cache) : ICacheService
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        byte[]? bytes = await cache.GetAsync(key, cancellationToken);
        return bytes is null ? default : JsonSerializer.Deserialize<T>(bytes, MessagingJson.Options);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration, CancellationToken cancellationToken)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(value, MessagingJson.Options);
        var entryOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration ?? DefaultExpiration,
        };
        return cache.SetAsync(key, bytes, entryOptions, cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken) => cache.RemoveAsync(key, cancellationToken);
}
