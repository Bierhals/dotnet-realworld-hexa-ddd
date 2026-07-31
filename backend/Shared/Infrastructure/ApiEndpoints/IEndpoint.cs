using Microsoft.AspNetCore.Routing;

namespace Conduit.Shared.Infrastructure.ApiEndpoints;

public interface IEndpoint
{
    public static abstract void MapEndpoint(IEndpointRouteBuilder app);
}
