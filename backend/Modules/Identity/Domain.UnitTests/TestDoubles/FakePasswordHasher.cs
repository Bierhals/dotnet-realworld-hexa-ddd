using Conduit.Identity.Domain.Services;

namespace Conduit.Identity.Domain.UnitTests.TestDoubles;

internal sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string plainPassword) => $"hashed:{plainPassword}";

    public bool Verify(string plainPassword, string hash) => hash == Hash(plainPassword);
}
