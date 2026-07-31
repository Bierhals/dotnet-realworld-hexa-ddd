using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Conduit.Identity.Application;
using Microsoft.Extensions.Options;

namespace Conduit.Host.WebApi.Infrastructure.Security;

public class JwtTokenGenerator(IOptions<JwtIssuerOptions> jwtOptions) : ITokenGenerator
{
    private readonly JwtIssuerOptions _jwtOptions = jwtOptions.Value;

    public string CreateToken(string username)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, _jwtOptions.JtiGenerator()),
            new Claim(
                JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(_jwtOptions.IssuedAt).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64
            ),
        };
        var jwt = new JwtSecurityToken(
            _jwtOptions.Issuer,
            _jwtOptions.Audience,
            claims,
            _jwtOptions.NotBefore,
            _jwtOptions.Expiration,
            _jwtOptions.SigningCredentials
        );

        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
        return encodedJwt;
    }
}
