using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Conduit.Features.Profiles;

public class Details
{
    public record Query([Required] string Username) : IRequest<ProfileEnvelope>;

    public class QueryHandler(IProfileReader profileReader)
        : IRequestHandler<Query, ProfileEnvelope>
    {
        public Task<ProfileEnvelope> Handle(Query message, CancellationToken cancellationToken) =>
            profileReader.ReadProfile(message.Username, cancellationToken);
    }
}
