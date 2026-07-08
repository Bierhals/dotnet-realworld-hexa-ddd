using System;
using System.Threading.Tasks;

namespace Conduit.Host.WebApi.Infrastructure.Security;

public interface IPasswordHasher : IDisposable
{
    public Task<byte[]> Hash(string password, byte[] salt);
}
