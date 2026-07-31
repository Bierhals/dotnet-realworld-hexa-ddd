using System;
using Conduit.Identity.Application;
using Conduit.Identity.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Identity.Infrastructure;

public static class IdentityModuleServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        services.AddIdentityPersistence(configureDbContext);
        services.AddIdentityApplication();

        services.AddScoped<IPasswordHasher, UserPasswordHasher>();

        return services;
    }
}
