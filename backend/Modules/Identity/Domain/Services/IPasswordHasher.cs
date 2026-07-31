using ErrorOr;

namespace Conduit.Identity.Domain.Services;

public interface IPasswordHasher
{
    public string Hash(string plainPassword);
    public bool Verify(string plainPassword, string hash);
}
