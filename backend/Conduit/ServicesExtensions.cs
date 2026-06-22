using System;
using System.Reflection;
using System.Threading.Tasks;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Security;
using Conduit.Shared.RequestHandling;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Articles = Conduit.Features.Articles;
using Comments = Conduit.Features.Comments;
using Favorites = Conduit.Features.Favorites;
using Followers = Conduit.Features.Followers;
using Profiles = Conduit.Features.Profiles;
using Tags = Conduit.Features.Tags;
using Users = Conduit.Features.Users;

namespace Conduit;

public static class ServicesExtensions
{
    public static void AddConduit(this IServiceCollection services)
    {
        services.AddValidation();


        services.AddTransient<Users.Create.Handler>();
        services.AddTransient<ICommandHandler<Users.Create.Command, Users.UserEnvelope>>(provider =>
        {
            var handler = provider.GetRequiredService<Users.Create.Handler>();
            var dbContext = provider.GetRequiredService<ConduitContext>();
            return new DBContextTransacionCommandDecorator<Users.Create.Command, Users.UserEnvelope>(dbContext, handler);
        });
        services.AddTransient<Users.Login.Handler>();
        services.AddTransient<ICommandHandler<Users.Login.Command, Users.UserEnvelope>>(provider =>
        {
            var handler = provider.GetRequiredService<Users.Login.Handler>();
            var dbContext = provider.GetRequiredService<ConduitContext>();
            return new DBContextTransacionCommandDecorator<Users.Login.Command, Users.UserEnvelope>(dbContext, handler);
        });
        services.AddTransient<IQueryHandler<Users.Details.Query, Users.UserEnvelope>, Users.Details.Handler>();
        services.AddTransient<Users.Edit.Handler>();
        services.AddTransient<ICommandHandler<Users.Edit.Command, Users.UserEnvelope>>(provider =>
        {
            var handler = provider.GetRequiredService<Users.Edit.Handler>();
            var dbContext = provider.GetRequiredService<ConduitContext>();
            return new DBContextTransacionCommandDecorator<Users.Edit.Command, Users.UserEnvelope>(dbContext, handler);
        });
        services.AddTransient<IQueryHandler<Tags.List.Query, Tags.TagsEnvelope>, Tags.List.QueryHandler>();
        services.AddTransient<IQueryHandler<Profiles.Details.Query, Profiles.ProfileEnvelope>, Profiles.Details.QueryHandler>();
        services.AddTransient<Followers.Add.Handler>();
        services.AddTransient<ICommandHandler<Followers.Add.Command, Profiles.ProfileEnvelope>>(provider =>
        {
            var handler = provider.GetRequiredService<Followers.Add.Handler>();
            var dbContext = provider.GetRequiredService<ConduitContext>();
            return new DBContextTransacionCommandDecorator<Followers.Add.Command, Profiles.ProfileEnvelope>(dbContext, handler);
        });
        services.AddTransient<Followers.Delete.Handler>();
        services.AddTransient<ICommandHandler<Followers.Delete.Command, Profiles.ProfileEnvelope>>(provider =>
        {
            var handler = provider.GetRequiredService<Followers.Delete.Handler>();
            var dbContext = provider.GetRequiredService<ConduitContext>();
            return new DBContextTransacionCommandDecorator<Followers.Delete.Command, Profiles.ProfileEnvelope>(dbContext, handler);
        });
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
        services.AddTransient<Articles.Create.Handler>();
        services.AddTransient<ICommandHandler<Articles.Create.Command, Articles.ArticleEnvelope>>(provider =>
        {
            var handler = provider.GetRequiredService<Articles.Create.Handler>();
            var dbContext = provider.GetRequiredService<ConduitContext>();
            return new DBContextTransacionCommandDecorator<Articles.Create.Command, Articles.ArticleEnvelope>(dbContext, handler);
        });
        services.AddTransient<Articles.Delete.Handler>();
        services.AddTransient<ICommandHandler<Articles.Delete.Command>>(provider =>
        {
            var handler = provider.GetRequiredService<Articles.Delete.Handler>();
            var dbContext = provider.GetRequiredService<ConduitContext>();
            return new DBContextTransacionCommandDecorator<Articles.Delete.Command>(dbContext, handler);
        });
        services.AddTransient<Articles.Edit.Handler>();
        services.AddTransient<ICommandHandler<Articles.Edit.Command, Articles.ArticleEnvelope>>(provider =>
        {
            var handler = provider.GetRequiredService<Articles.Edit.Handler>();
            var dbContext = provider.GetRequiredService<ConduitContext>();
            return new DBContextTransacionCommandDecorator<Articles.Edit.Command, Articles.ArticleEnvelope>(dbContext, handler);
        });

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        services.AddScoped<Profiles.IProfileReader, Profiles.ProfileReader>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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
