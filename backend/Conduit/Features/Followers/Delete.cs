using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Features.Profiles;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using Conduit.Shared.RequestHandling;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Followers;

public class Delete
{
    public record Command([Required] string Username) : ICommand<ProfileEnvelope>;

    public class Handler(
        ConduitContext context,
        ICurrentUserAccessor currentUserAccessor,
        IProfileReader profileReader
    ) : ICommandHandler<Command, ProfileEnvelope>
    {
        public async Task<ProfileEnvelope> Handle(
            Command message,
            CancellationToken cancellationToken
        )
        {
            var target = await context.Persons.FirstOrDefaultAsync(
                x => x.Username == message.Username,
                cancellationToken
            );

            if (target is null)
            {
                throw new RestException(
                    HttpStatusCode.NotFound,
                    new { User = Constants.NOT_FOUND }
                );
            }

            var observer = await context.Persons.FirstOrDefaultAsync(
                x => x.Username == currentUserAccessor.GetCurrentUsername(),
                cancellationToken
            );

            if (observer is null)
            {
                throw new RestException(
                    HttpStatusCode.NotFound,
                    new { User = Constants.NOT_FOUND }
                );
            }

            var followedPeople = await context.FollowedPeople.FirstOrDefaultAsync(
                x => x.ObserverId == observer.PersonId && x.TargetId == target.PersonId,
                cancellationToken
            );

            if (followedPeople != null)
            {
                context.FollowedPeople.Remove(followedPeople);
                await context.SaveChangesAsync(cancellationToken);
            }

            return await profileReader.ReadProfile(message.Username, cancellationToken);
        }
    }
}
