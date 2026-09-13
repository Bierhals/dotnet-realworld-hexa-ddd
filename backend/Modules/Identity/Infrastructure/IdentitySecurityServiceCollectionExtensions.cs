using Conduit.Identity.Domain.Services;
using Conduit.Identity.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Identity.Infrastructure;

public static class IdentitySecurityServiceCollectionExtensions
{
    /// <summary>
    /// How this module turns a plain password into something storable and verifies it again. A
    /// concern of its own rather than part of persistence: the hash happens to be stored, but
    /// choosing the algorithm has nothing to do with how users are read or written.
    /// </summary>
    public static IServiceCollection AddIdentitySecurity(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, UserPasswordHasher>();

        return services;
    }
}
