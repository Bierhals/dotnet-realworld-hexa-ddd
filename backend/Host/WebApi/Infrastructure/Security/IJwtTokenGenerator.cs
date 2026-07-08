namespace Conduit.Host.WebApi.Infrastructure.Security;

public interface IJwtTokenGenerator
{
    public string CreateToken(string username);
}
