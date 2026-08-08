using Conduit.Shared.Application;

namespace Conduit.Articles.Application.UnitTests.TestDoubles;

internal sealed class StubCurrentUserAccessor(string? username) : ICurrentUserAccessor
{
    public string? GetCurrentUsername() => username;
}
