namespace Conduit.Identity.Application.UnitTests.TestDoubles;

internal sealed class StubTokenGenerator : ITokenGenerator
{
    public string CreateToken(string username) => $"token-for-{username}";
}
