using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Host.WebApi.Features.Profiles;

public interface IProfileReader
{
    public Task<ProfileEnvelope> ReadProfile(string username, CancellationToken cancellationToken);
}
