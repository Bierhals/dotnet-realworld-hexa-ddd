using Microsoft.AspNetCore.Routing;

namespace Conduit.Shared.ApiEndpoints;

public interface IEndpoint
{
    static abstract void MapEndpoint(IEndpointRouteBuilder app);
}
