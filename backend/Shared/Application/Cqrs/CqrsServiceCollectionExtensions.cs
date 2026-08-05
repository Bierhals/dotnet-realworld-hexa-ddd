using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Conduit.Shared.Application.Cqrs;

public static class CqrsServiceCollectionExtensions
{
    /// <summary>
    /// Registers the CQRS mediator. Every module that dispatches commands or queries calls this
    /// from its own Add{Module}Application(), so the registration stays additive no matter how
    /// many modules the Host composes.
    /// </summary>
    public static IServiceCollection AddCqrsMediator(this IServiceCollection services)
    {
        services.TryAddScoped<ICqrsMediator, CqrsMediator>();

        return services;
    }
}
