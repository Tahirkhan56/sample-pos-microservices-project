using Microsoft.AspNetCore.Routing;

namespace Pos.BuildingBlocks.Web.Endpoints;

/// <summary>A self-registering group of Minimal API endpoints. Discovered by <see cref="EndpointExtensions"/>.</summary>
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
