using Microsoft.AspNetCore.Routing;

namespace Conduit.Shared.ApiEndpoints;

public static class EndpointsExtensions
{
    public static IEndpointRouteBuilder AddEndpoint<TEndpoint>(this IEndpointRouteBuilder app) where TEndpoint : IEndpoint
    {
        TEndpoint.MapEndpoint(app);
        return app;
    }
}
