using Conduit.Articles.Application;
using Conduit.Articles.Infrastructure.Adapters;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Articles.Infrastructure;

public static class ArticlesAdaptersServiceCollectionExtensions
{
    /// <summary>
    /// The adapters onto the modules this one reads from. They are the only types in Articles that
    /// reference another module's contracts, which is why they are registered apart from the
    /// module's own persistence.
    /// </summary>
    public static IServiceCollection AddArticlesAdapters(this IServiceCollection services)
    {
        services.AddScoped<IProfileReader, IdentityProfileReaderAdapter>();

        return services;
    }
}
