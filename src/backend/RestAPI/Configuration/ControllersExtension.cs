using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.RestAPI.Configuration;

static class ControllersExtension
{
    public static IServiceCollection AddConduitControllers(this IServiceCollection services)
    {
        services.AddControllers();

        return services;
    }

    public static WebApplication UseConduitControllers(this WebApplication app)
    {
        app.MapControllers();

        return app;
    }
}