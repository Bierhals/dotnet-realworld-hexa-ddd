namespace Conduit.Identity.Application;

public interface ITokenGenerator
{
    public string CreateToken(string username);
}
