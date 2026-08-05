using System;
using System.Reflection;
using System.Threading.Tasks;
using Conduit.Host.WebApi.Infrastructure;
using Conduit.Host.WebApi.Infrastructure.Security;
using Conduit.Host.WebApi.Shared.RequestHandling;
using Conduit.Identity.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Articles = Conduit.Host.WebApi.Features.Articles;
using Comments = Conduit.Host.WebApi.Features.Comments;
using Favorites = Conduit.Host.WebApi.Features.Favorites;

namespace Conduit.Host.WebApi;

public static class ServicesExtensions
{
    public static void AddConduit(this IServiceCollection services)
    {
        services.AddValidation();

        services.AddTransient<Favorites.Add.Handler>();
        services.AddTransient<ICommandHandler<Favorites.Add.Command, Articles.ArticleEnvelope>>(provider =>
        {
            var handler = provider.GetRequiredService<Favorites.Add.Handler>();
            var dbContext = provider.GetRequiredService<ConduitContext>();
            return new DBContextTransacionCommandDecorator<Favorites.Add.Command, Articles.ArticleEnvelope>(dbContext, handler);
        });
        services.AddTransient<Favorites.Delete.Handler>();
        services.AddTransient<ICommandHandler<Favorites.Delete.Command, Articles.ArticleEnvelope>>(provider =>
        {
            var handler = provider.GetRequiredService<Favorites.Delete.Handler>();
            var dbContext = provider.GetRequiredService<ConduitContext>();
            return new DBContextTransacionCommandDecorator<Favorites.Delete.Command, Articles.ArticleEnvelope>(dbContext, handler);
        });
        services.AddTransient<IQueryHandler<Comments.List.Query, Comments.CommentsEnvelope>, Comments.List.Handler>();
        services.AddTransient<Comments.Create.Handler>();
        services.AddTransient<ICommandHandler<Comments.Create.Command, Comments.CommentEnvelope>>(provider =>
        {
            var handler = provider.GetRequiredService<Comments.Create.Handler>();
            var dbContext = provider.GetRequiredService<ConduitContext>();
            return new DBContextTransacionCommandDecorator<Comments.Create.Command, Comments.CommentEnvelope>(dbContext, handler);
        });
        services.AddTransient<Comments.Delete.Handler>();
        services.AddTransient<ICommandHandler<Comments.Delete.Command>>(provider =>
        {
            var handler = provider.GetRequiredService<Comments.Delete.Handler>();
            var dbContext = provider.GetRequiredService<ConduitContext>();
            return new DBContextTransacionCommandDecorator<Comments.Delete.Command>(dbContext, handler);
        });
        services.AddTransient<IQueryHandler<Articles.List.Query, Articles.ArticlesEnvelope>, Articles.List.Handler>();
        services.AddTransient<IQueryHandler<Articles.Details.Query, Articles.ArticleEnvelope>, Articles.Details.Handler>();

        // The article commands are deliberately NOT wrapped in DBContextTransacionCommandDecorator.
        // Each of them persists through a single SaveChangesAsync, which EF Core already runs in
        // its own transaction, so the decorator would add no atomicity. It would, however, hold an
        // open write transaction on ConduitContext for the whole handler - and these handlers also
        // talk to the Tags module, which writes through its own DbContext and connection. On SQLite
        // (both contexts share one file) that second connection cannot get a write lock and the
        // request fails with "database is locked".
        services.AddTransient<ICommandHandler<Articles.Create.Command, Articles.ArticleEnvelope>, Articles.Create.Handler>();
        services.AddTransient<ICommandHandler<Articles.Delete.Command>, Articles.Delete.Handler>();
        services.AddTransient<ICommandHandler<Articles.Edit.Command, Articles.ArticleEnvelope>, Articles.Edit.Handler>();

        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.AddScoped<CurrentUserContext>();
        services.AddScoped<Conduit.Shared.Application.ICurrentUserAccessor>(provider =>
            provider.GetRequiredService<CurrentUserContext>());
        services.AddScoped<Conduit.Shared.Infrastructure.ICurrentUserSetter>(provider =>
            provider.GetRequiredService<CurrentUserContext>());
    }

    public static void AddJwt(this IServiceCollection services)
    {
        services.AddOptions();

        var signingKey = new SymmetricSecurityKey(
            "somethinglongerforthisdumbalgorithmisrequired"u8.ToArray()
        );
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var issuer = "issuer";
        var audience = "audience";

        services.Configure<JwtIssuerOptions>(options =>
        {
            options.Issuer = issuer;
            options.Audience = audience;
            options.SigningCredentials = signingCredentials;
        });

        var tokenValidationParameters = new TokenValidationParameters
        {
            // The signing key must match!
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingCredentials.Key,
            // Validate the JWT Issuer (iss) claim
            ValidateIssuer = true,
            ValidIssuer = issuer,
            // Validate the JWT Audience (aud) claim
            ValidateAudience = true,
            ValidAudience = audience,
            // Validate the token expiry
            ValidateLifetime = true,
            // If you want to allow a certain amount of clock drift, set that here:
            ClockSkew = TimeSpan.Zero,
        };

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = (context) =>
                    {
                        var token = context.HttpContext.Request.Headers.Authorization.ToString();
                        if (token.StartsWith("Token ", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = token["Token ".Length..].Trim();
                        }

                        return Task.CompletedTask;
                    },
                };
            });
    }
}
