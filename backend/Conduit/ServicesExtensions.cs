using System;
using System.Reflection;
using System.Threading.Tasks;
using Conduit.Features.Profiles;
using Conduit.Features.Users;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Security;
using Conduit.Shared.RequestHandling;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Conduit;

public static class ServicesExtensions
{
    public static void AddConduit(this IServiceCollection services)
    {
        services.AddValidation();
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
        );
        services.AddScoped(
            typeof(IPipelineBehavior<,>),
            typeof(DBContextTransactionPipelineBehavior<,>)
        );

        services.AddTransient<Create.Handler>();
        services.AddTransient<ICommandHandler<Create.Command, UserEnvelope>>(provider =>
        {
            var handler = provider.GetRequiredService<Create.Handler>();
            var dbContext = provider.GetRequiredService<ConduitContext>();
            return new DBContextTransacionCommandDecorator<Create.Command, UserEnvelope>(dbContext, handler);
        });
        services.AddTransient<Login.Handler>();
        services.AddTransient<ICommandHandler<Login.Command, UserEnvelope>>(provider =>
        {
            var handler = provider.GetRequiredService<Login.Handler>();
            var dbContext = provider.GetRequiredService<ConduitContext>();
            return new DBContextTransacionCommandDecorator<Login.Command, UserEnvelope>(dbContext, handler);
        });
        services.AddTransient<IQueryHandler<Features.Users.Details.Query, UserEnvelope>, Features.Users.Details.Handler>();
        services.AddTransient<Edit.Handler>();
        services.AddTransient<ICommandHandler<Edit.Command, UserEnvelope>>(provider =>
        {
            var handler = provider.GetRequiredService<Edit.Handler>();
            var dbContext = provider.GetRequiredService<ConduitContext>();
            return new DBContextTransacionCommandDecorator<Edit.Command, UserEnvelope>(dbContext, handler);
        });

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        services.AddScoped<IProfileReader, ProfileReader>();
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
