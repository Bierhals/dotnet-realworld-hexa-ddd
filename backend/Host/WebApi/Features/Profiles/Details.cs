using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Host.WebApi.Shared.RequestHandling;

namespace Conduit.Host.WebApi.Features.Profiles;

public class Details
{
    public record Query([Required] string Username) : IQuery<ProfileEnvelope>;

    public class QueryHandler(IProfileReader profileReader)
        : IQueryHandler<Query, ProfileEnvelope>
    {
        public Task<ProfileEnvelope> Handle(Query message, CancellationToken cancellationToken) =>
            profileReader.ReadProfile(message.Username, cancellationToken);
    }
}
