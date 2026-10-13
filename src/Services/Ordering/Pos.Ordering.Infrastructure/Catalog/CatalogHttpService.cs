using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Pos.BuildingBlocks.Domain.Results;
using Pos.Ordering.Application.Abstractions;

namespace Pos.Ordering.Infrastructure.Catalog;

/// <summary>Typed HTTP client for the Catalog API. Retries, timeouts and circuit breaking come from the standard resilience handler.</summary>
internal sealed partial class CatalogHttpService(HttpClient httpClient, ILogger<CatalogHttpService> logger) : ICatalogService
{
    private static readonly Error Unavailable =
        Error.Failure("Catalog.Unavailable", "The catalog service is currently unavailable. Please retry.");

    public async Task<Result<IReadOnlyList<CatalogProduct>>> GetProductsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken)
    {
        string query = string.Join('&', productIds.Select(id => $"ids={id}"));

        try
        {
            List<CatalogProduct>? products = await httpClient.GetFromJsonAsync<List<CatalogProduct>>(
                new Uri($"api/v1/products/lookup?{query}", UriKind.Relative),
                cancellationToken);

            return products ?? [];
        }
        catch (HttpRequestException exception)
        {
            LogCatalogFailure(logger, exception);
            return Unavailable;
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Catalog lookup failed")]
    private static partial void LogCatalogFailure(ILogger logger, Exception exception);
}
