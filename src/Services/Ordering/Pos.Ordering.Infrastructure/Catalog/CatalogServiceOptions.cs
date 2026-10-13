using System.ComponentModel.DataAnnotations;

namespace Pos.Ordering.Infrastructure.Catalog;

public sealed class CatalogServiceOptions
{
    public const string SectionName = "Services:Catalog";

    /// <summary>Base URL of the Catalog API, e.g. <c>http://catalog-api:8080</c> inside Kubernetes.</summary>
    [Required]
    [Url]
    public string BaseUrl { get; init; } = string.Empty;
}
