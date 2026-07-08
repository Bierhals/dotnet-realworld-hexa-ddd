using Conduit.Host.WebApi.Infrastructure;

namespace Conduit.Host.WebApi.IntegrationTests;

public class StubCurrentUserAccessor(string userName) : ICurrentUserAccessor
{
    public string GetCurrentUsername() => userName;
}
